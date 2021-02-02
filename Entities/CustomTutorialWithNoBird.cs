using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomTutorialWithNoBird")]
    [TrackedAs(typeof(CustomBirdTutorial))]
    class CustomTutorialWithNoBird : CustomBirdTutorial {
        public static void Load() {
            On.Celeste.BirdNPC.StartleAndFlyAway += killFlyAwayEffects;
            IL.Celeste.BirdTutorialGui.Render += togglePointer;
        }

        public static void Unload() {
            On.Celeste.BirdNPC.StartleAndFlyAway -= killFlyAwayEffects;
            IL.Celeste.BirdTutorialGui.Render -= togglePointer;
        }

        private readonly bool hasPointer;

        public CustomTutorialWithNoBird(EntityData data, Vector2 offset) : base(data, offset) {
            hasPointer = data.Bool("hasPointer", true);

            // no bird allowed!
            Remove(Sprite);
        }

        private static IEnumerator killFlyAwayEffects(On.Celeste.BirdNPC.orig_StartleAndFlyAway orig, BirdNPC self) {
            if (self is CustomTutorialWithNoBird) {
                // don't play any flying away effect since there is no bird!
                yield break;
            }

            // just let the original routine go.
            IEnumerator origEnum = orig(self);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }
        }

        private static void togglePointer(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're looking for a for loop looping 36 times.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldloc_S, instr => instr.MatchLdcI4(36))) {
                Logger.Log("MaxHelpingHand/CustomTutorialWithNoBird", $"Modding tutorial bird bubble pointer at {cursor.Index} in IL for BirdTutorialGui.Render");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, BirdTutorialGui, int>>((orig, self) => {
                    if (self.Entity is CustomTutorialWithNoBird tutorial && !tutorial.hasPointer) {
                        // remove the pointer by breaking the for loop drawing it.
                        return -1;
                    }
                    return orig;
                });
            }
        }
    }
}
