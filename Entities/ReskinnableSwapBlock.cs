using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReskinnableSwapBlock")]
    [TrackedAs(typeof(SwapBlock))]
    public class ReskinnableSwapBlock : SwapBlock {
        private static Type pathRendererType = typeof(SwapBlock).GetNestedType("PathRenderer", BindingFlags.NonPublic);
        private static ILHook hookOnPathRendererConstructor;

        public static void Load() {
            On.Celeste.SwapBlock.ctor_EntityData_Vector2 += onSwapBlockConstruct;
            IL.Celeste.SwapBlock.ctor_Vector2_float_float_Vector2_Themes += modSwapBlockTexturesInConstructor;

            hookOnPathRendererConstructor = new ILHook(pathRendererType.GetConstructor(new Type[] { typeof(SwapBlock) }), modSwapBlockTexturesInPathRenderer);
        }

        public static void Unload() {
            On.Celeste.SwapBlock.ctor_EntityData_Vector2 -= onSwapBlockConstruct;
            IL.Celeste.SwapBlock.ctor_Vector2_float_float_Vector2_Themes -= modSwapBlockTexturesInConstructor;

            hookOnPathRendererConstructor?.Dispose();
            hookOnPathRendererConstructor = null;
        }

        private static void onSwapBlockConstruct(On.Celeste.SwapBlock.orig_ctor_EntityData_Vector2 orig, SwapBlock self, EntityData data, Vector2 offset) {
            // we are using a hook rather than the constructor, because we want to run our code before the base constructor.
            if (self is ReskinnableSwapBlock swapBlock) {
                swapBlock.spriteDirectory = data.Attr("spriteDirectory", "objects/swapblock");
            }

            orig(self, data, offset);
        }

        private static void modSwapBlockTexturesInConstructor(ILContext il) {
            modSwapBlockTextures(il, false);
        }

        private static void modSwapBlockTexturesInPathRenderer(ILContext il) {
            modSwapBlockTextures(il, true);
        }

        private static void modSwapBlockTextures(ILContext il, bool isPathRenderer) {
            ILCursor cursor = new ILCursor(il);

            string[] stringsToLookUp = { "objects/swapblock/block", "objects/swapblock/blockRed", "objects/swapblock/target", "objects/swapblock/path" };
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldstr && stringsToLookUp.Contains((string) instr.Operand))) {
                Logger.Log("MaxHelpingHand/ReskinnableSwapBlock", $"Reskinning swap block at {cursor.Index} in IL for SwapBlock constructor (path renderer = {isPathRenderer})");

                cursor.Emit(OpCodes.Ldarg_0);
                if (isPathRenderer) {
                    cursor.Emit(OpCodes.Ldfld, pathRendererType.GetField("block", BindingFlags.NonPublic | BindingFlags.Instance));
                }
                cursor.EmitDelegate<Func<string, SwapBlock, string>>((orig, self) => {
                    if (self is ReskinnableSwapBlock swapBlock) {
                        return orig.Replace("objects/swapblock", swapBlock.spriteDirectory);
                    }
                    return orig;
                });
            }
        }

        private string spriteDirectory;
        private ParticleType customParticleColor;

        public ReskinnableSwapBlock(EntityData data, Vector2 offset) : base(data, offset) {
            DynData<SwapBlock> self = new DynData<SwapBlock>(this);

            // replace the sprites for the middle of the blocks.
            Sprite middleGreen = self.Get<Sprite>("middleGreen");
            middleGreen.Reset(GFX.Game, spriteDirectory + "/");
            middleGreen.AddLoop("idle", "midBlock", 0.08f);
            middleGreen.Play("idle");

            Sprite middleRed = self.Get<Sprite>("middleRed");
            middleRed.Reset(GFX.Game, spriteDirectory + "/");
            middleRed.AddLoop("idle", "midBlockRed", 0.08f);
            middleRed.Play("idle");

            customParticleColor = new ParticleType(P_Move) {
                Color = Calc.HexToColor(data.Attr("particleColor1", "fbf236")),
                Color2 = Calc.HexToColor(data.Attr("particleColor2", "6abe30"))
            };
        }

        public override void Update() {
            ParticleType oldMove = P_Move;
            P_Move = customParticleColor;

            base.Update();

            P_Move = oldMove;
        }
    }
}
