using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    // Northern Lights, but with 6 more IL hooks!
    class NorthernLightsCustomColors : NorthernLights {
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

        private static void hookConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // hook #1: change the northern lights colors in the constructor.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdsfld<NorthernLights>("colors"))) {
                Logger.Log("MaxHelpingHand/NorthernLightsCustomColors", $"Patching colors in {cursor.Index} in IL for NorthernLights constructor");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Color[], NorthernLights, Color[]>>((orig, self) => {
                    if (self is NorthernLightsCustomColors) {
                        return Colors;
                    }
                    return orig;
                });
            }

            cursor.Index = 0;

            // hook #2: change the background top color in the constructor.
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("020825"))) {
                Logger.Log("MaxHelpingHand/NorthernLightsCustomColors", $"Patching gradient color 1 in {cursor.Index} in IL for NorthernLights constructor");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<string, NorthernLights, string>>((orig, self) => {
                    if (self is NorthernLightsCustomColors) {
                        return GradientColor1;
                    }
                    return orig;
                });
            }

            // hook #3: change the background bottom color in the constructor.
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("170c2f"))) {
                Logger.Log("MaxHelpingHand/NorthernLightsCustomColors", $"Patching gradient color 2 in {cursor.Index} in IL for NorthernLights constructor");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<string, NorthernLights, string>>((orig, self) => {
                    if (self is NorthernLightsCustomColors) {
                        return GradientColor2;
                    }
                    return orig;
                });
            }
        }

        private static void hookBeforeRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // hook #4: if the backdrop is transparent, make sure we clean it up, or else frames will "stack up".
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall(typeof(GFX), "DrawVertices"))) {
                Logger.Log("MaxHelpingHand/NorthernLightsCustomColors", $"Cleaning background at {cursor.Index} in IL for NorthernLights.BeforeRender");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<NorthernLights>>(self => {
                    if (self is NorthernLightsCustomColors custom && !custom.displayBackground) {
                        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                    }
                });
            }

            // hook #5: clean up the gaussian blur buffer when using it when we have transparency (no background).
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
        }

        private static void modStrandReset(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // hook #6: use the static variable defined in NorthernLightsCustomColors instead of vanilla colors.
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
