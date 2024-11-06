using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class AllSideTentacles : Tentacles {
        public static void Load() {
            IL.Celeste.Tentacles.Update += modTentaclesUpdate;
        }

        public static void Unload() {
            IL.Celeste.Tentacles.Update -= modTentaclesUpdate;
        }

        private Side side;

        public AllSideTentacles(Side side, Color color, float outwardsOffset) : base(side, color, outwardsOffset) {
            this.side = side;
        }

        private static void modTentaclesUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're injecting ourselves to replace the value of the first variable of the method (which is the player position-dependent offset).
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0f))) {
                Logger.Log("MaxHelpingHand/AllSideTentacles", $"Handling Left and Top tentacles at {cursor.Index} in IL for Tentacles.Update");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<float, Tentacles, Scene, float>>((orig, self, scene) => {
                    if (self is AllSideTentacles allSideSelf && self.IsVisible(scene as Level)) {
                        // replicate the code vanilla already has for Right and Bottom.
                        Camera camera = (scene as Level).Camera;
                        Player player = scene.Tracker.GetEntity<Player>();

                        if (player != null) {
                            if (allSideSelf.side == Side.Left) {
                                return (player.X - camera.X) - (GameplayBuffers.Gameplay.Width / 2f);
                            } else if (allSideSelf.side == Side.Top) {
                                return player.Y - camera.Y - GameplayBuffers.Gameplay.Height;
                            }
                        }
                    }

                    // vanilla tentacles or Right/Bottom tentacles => nothing to do
                    return orig;
                });
            }
        }
    }
}
