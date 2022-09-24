using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class NorthernLightsCustomColors : NorthernLights {
        private static ILHook strandILHook;

        public static void Load() {
            IL.Celeste.NorthernLights.ctor += hookConstructor;
            IL.Celeste.NorthernLights.BeforeRender += hookBeforeRender;
            strandILHook = new ILHook(typeof(NorthernLights).GetNestedType("Strand", BindingFlags.NonPublic).GetMethod("Reset"), modStrandReset);
        }

        public static void Unload() {
            IL.Celeste.NorthernLights.ctor -= hookConstructor;
            IL.Celeste.NorthernLights.BeforeRender -= hookBeforeRender;
            strandILHook?.Dispose();
            strandILHook = null;
        }

        public static string GradientColor1;
        public static string GradientColor2;
        public static Color[] Colors;
        public static int ParticleCount;
        public static int StrandCount;

        private static void hookConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            replacerHook(cursor, cursor => cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld<NorthernLights>("colors")), 0, () => Colors);
            replacerHook(cursor, cursor => cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("020825")), 0, () => GradientColor1);
            replacerHook(cursor, cursor => cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("170c2f")), 0, () => GradientColor2);
            replacerHook(cursor, cursor => cursor.TryGotoNext(instr => instr.MatchLdcI4(50), instr => instr.OpCode == OpCodes.Newarr), 1, () => ParticleCount);
            replacerHook(cursor, cursor => cursor.TryGotoNext(instr => instr.MatchLdcI4(3), instr => instr.OpCode == OpCodes.Blt_S), 1, () => StrandCount);

            // resize the array holding the vertices to accomodate with the strand count, to avoid out of bounds exceptions.
            replacerHook(cursor, cursor => cursor.TryGotoNext(instr => instr.MatchLdcI4(1024), instr => instr.OpCode == OpCodes.Newarr), 1, () => 234 * StrandCount);
        }

        /**
         * The given condition should move the cursor after a method returning a T (optionally using the offset to do that).
         * Then, if "this" is a NorthernLightsCustomColors, the return value of the method will be replaced with what replaceWith returns.
         */
        private static void replacerHook<T>(ILCursor cursor, Func<ILCursor, bool> condition, int offset, Func<T> replaceWith) {
            while (condition(cursor)) {
                Logger.Log("MaxHelpingHand/NorthernLightsCustomColors", $"Applying patch in {cursor.Index} in IL for NorthernLights constructor");

                cursor.Index += offset;

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<T, NorthernLights, T>>((orig, self) => {
                    if (self is NorthernLightsCustomColors) {
                        return replaceWith();
                    }
                    return orig;
                });
            }

            cursor.Index = 0;
        }

        private static void hookBeforeRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // if the backdrop is transparent, make sure we clean it up, or else frames will "stack up".
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall(typeof(GFX), "DrawVertices"))) {
                Logger.Log("MaxHelpingHand/NorthernLightsCustomColors", $"Cleaning background at {cursor.Index} in IL for NorthernLights.BeforeRender");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<NorthernLights>>(self => {
                    if (self is NorthernLightsCustomColors custom && !custom.displayBackground) {
                        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                    }
                });
            }

            // clean up the gaussian blur buffer when using it when we have transparency (no background).
            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchCall(typeof(GFX), "DrawVertices"),
                instr => instr.MatchLdcI4(0) || (instr.MatchNop() && instr.Next.MatchLdcI4(0)))) {

                // yeah that nop sure was essential, thanks Steam FNA
                if (cursor.Prev.OpCode == OpCodes.Nop) cursor.Index++;

                Logger.Log("MaxHelpingHand/NorthernLightsCustomColors", $"Fixing gaussian blur cleanup at {cursor.Index} in IL for NorthernLights.BeforeRender");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<bool, NorthernLights, bool>>((orig, self) => {
                    if (self is NorthernLightsCustomColors custom && !custom.displayBackground) {
                        return true;
                    }
                    return orig;
                });
            }

            cursor.Index = 0;

            // skip DrawVertices if there is no vertex to draw because that crashes on XNA
            if (cursor.TryGotoNext(instr => instr.MatchLdfld<NorthernLights>("verts"))
                && cursor.TryGotoNext(instr => instr.OpCode == OpCodes.Call && (instr.Operand as MethodReference).Name == "DrawVertices")) {

                Logger.Log("MaxHelpingHand/NorthernLightsCustomColors", $"Skipping DrawVertices if there is no vertex to draw {cursor.Index} in IL for NorthernLights.BeforeRender");

                // if (verts.Length != 0), proceed with DrawVertices.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(NorthernLights).GetField("verts", BindingFlags.NonPublic | BindingFlags.Instance));
                cursor.Emit(OpCodes.Ldlen);
                cursor.Emit(OpCodes.Ldc_I4_0);
                cursor.Emit(OpCodes.Bne_Un, cursor.Next);

                // ... else, throw out all parameters and skip the call.
                for (int i = 0; i < 5; i++) {
                    cursor.Emit(OpCodes.Pop);
                }
                cursor.Emit(OpCodes.Br, cursor.Next.Next);
            }
        }

        private static void modStrandReset(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // use the static variable defined in NorthernLightsCustomColors instead of vanilla colors.
            // this static variable is filled when a custom northern lights BG is updated, and cleared when it's done.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld<NorthernLights>("colors"))) {
                Logger.Log("MaxHelpingHand/NorthernLightsCustomColors", $"Patching colors in {cursor.Index} in IL for NorthernLights.Strand.Reset");

                cursor.EmitDelegate<Func<Color[], Color[]>>(orig => {
                    if (Colors != null) {
                        return Colors;
                    }
                    return orig;
                });
            }
        }


        private readonly Color[] colors;
        private readonly bool displayBackground;

        public NorthernLightsCustomColors(Color[] colors, bool displayBackground) : base() {
            this.colors = colors;
            this.displayBackground = displayBackground;
        }

        public override void Update(Scene scene) {
            // the Colors variable is caught by hook #6.
            Colors = colors;
            base.Update(scene);
            Colors = null;
        }
    }
}
