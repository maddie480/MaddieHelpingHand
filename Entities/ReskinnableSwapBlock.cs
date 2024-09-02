using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
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

            IL.Celeste.SwapBlock.OnDash += modSwapBlockSounds;
            IL.Celeste.SwapBlock.Update += modSwapBlockSounds;

            hookOnPathRendererConstructor = new ILHook(pathRendererType.GetConstructor(new Type[] { typeof(SwapBlock) }), modSwapBlockTexturesInPathRenderer);
        }

        public static void Unload() {
            On.Celeste.SwapBlock.ctor_EntityData_Vector2 -= onSwapBlockConstruct;
            IL.Celeste.SwapBlock.ctor_Vector2_float_float_Vector2_Themes -= modSwapBlockTexturesInConstructor;

            IL.Celeste.SwapBlock.OnDash -= modSwapBlockSounds;
            IL.Celeste.SwapBlock.Update -= modSwapBlockSounds;

            hookOnPathRendererConstructor?.Dispose();
            hookOnPathRendererConstructor = null;
        }

        private static void onSwapBlockConstruct(On.Celeste.SwapBlock.orig_ctor_EntityData_Vector2 orig, SwapBlock self, EntityData data, Vector2 offset) {
            // we are using a hook rather than the constructor, because we want to run our code before the base constructor.
            if (self is ReskinnableSwapBlock swapBlock) {
                swapBlock.spriteDirectory = data.Attr("spriteDirectory", "objects/swapblock");
                swapBlock.transparentBackground = data.Bool("transparentBackground");
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
                        if (swapBlock.transparentBackground && orig == "objects/swapblock/path") {
                            return orig.Replace("objects/swapblock", "MaxHelpingHand/swapblocktransparentbg");
                        } else {
                            return orig.Replace("objects/swapblock", swapBlock.spriteDirectory);
                        }
                    }
                    return orig;
                });
            }
        }

        private static void modSwapBlockSounds(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            Dictionary<string, Func<ReskinnableSwapBlock, string>> stuffToLookUp = new Dictionary<string, Func<ReskinnableSwapBlock, string>>() {
                { "event:/game/05_mirror_temple/swapblock_move", block => block.moveSound },
                { "event:/game/05_mirror_temple/swapblock_move_end", block => block.moveEndSound },
                { "event:/game/05_mirror_temple/swapblock_return", block => block.returnSound },
                { "event:/game/05_mirror_temple/swapblock_return_end", block => block.returnEndSound }
            };

            foreach (KeyValuePair<string, Func<ReskinnableSwapBlock, string>> replace in stuffToLookUp) {
                cursor.Index = 0;

                while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr(replace.Key))) {
                    Logger.Log("MaxHelpingHand/ReskinnableSwapBlock", $"Changing sounds of swap block at {cursor.Index} in IL for {il.Method.Name} ({replace.Key})");

                    Func<ReskinnableSwapBlock, string> valueGetter = replace.Value;

                    cursor.Emit(OpCodes.Ldarg_0);
                    cursor.EmitDelegate<Func<string, SwapBlock, string>>((orig, self) => {
                        if (self is ReskinnableSwapBlock swapBlock) {
                            return valueGetter(swapBlock);
                        }
                        return orig;
                    });
                }
            }
        }

        private string spriteDirectory;
        private ParticleType customParticleColor;
        private string moveSound;
        private string moveEndSound;
        private string returnSound;
        private string returnEndSound;
        private bool transparentBackground;

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

            moveSound = data.Attr("moveSound", defaultValue: "event:/game/05_mirror_temple/swapblock_move");
            moveEndSound = data.Attr("moveEndSound", defaultValue: "event:/game/05_mirror_temple/swapblock_move_end");
            returnSound = data.Attr("returnSound", defaultValue: "event:/game/05_mirror_temple/swapblock_return");
            returnEndSound = data.Attr("returnEndSound", defaultValue: "event:/game/05_mirror_temple/swapblock_return_end");
        }

        public override void Update() {
            ParticleType oldMove = P_Move;
            P_Move = customParticleColor;

            base.Update();

            P_Move = oldMove;
        }
    }
}
