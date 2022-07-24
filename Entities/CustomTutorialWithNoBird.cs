using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomTutorialWithNoBird")]
    [TrackedAs(typeof(CustomBirdTutorial))]
    public class CustomTutorialWithNoBird : CustomBirdTutorial {
        private enum Direction { Up, Down, Left, Right, None }

        public static void Load() {
            On.Celeste.BirdNPC.StartleAndFlyAway += killFlyAwayEffects;
            IL.Celeste.BirdTutorialGui.Render += replacePointerIfNeeded;
        }

        public static void Unload() {
            On.Celeste.BirdNPC.StartleAndFlyAway -= killFlyAwayEffects;
            IL.Celeste.BirdTutorialGui.Render -= replacePointerIfNeeded;
        }

        private readonly Direction direction;

        public CustomTutorialWithNoBird(EntityData data, Vector2 offset) : base(data, offset) {
            if (!data.Bool("hasPointer", true)) {
                // backwards compatibility, having "Has Pointer" checked in the old version is the same as setting "Direction" to "None".
                direction = Direction.None;
            } else {
                direction = data.Enum("direction", Direction.Down);
            }

            // no bird allowed!
            Remove(Sprite);
        }

        private static IEnumerator killFlyAwayEffects(On.Celeste.BirdNPC.orig_StartleAndFlyAway orig, BirdNPC self) {
            if (self is CustomTutorialWithNoBird) {
                // don't play any flying away effect since there is no bird!
                yield break;
            }

            // just let the original routine go.
            yield return new SwapImmediately(orig(self));
        }

        private static void replacePointerIfNeeded(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're looking for a for loop looping 36 times.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldloc_S, instr => instr.MatchLdcI4(36))) {
                Logger.Log("MaxHelpingHand/CustomTutorialWithNoBird", $"Modding tutorial bird bubble pointer at {cursor.Index} in IL for BirdTutorialGui.Render");

                cursor.Emit(OpCodes.Ldarg_0);

                // load a bunch of private variables to get them as function parameters instead of having to get them through reflection...

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(BirdTutorialGui).GetField("controlsWidth", BindingFlags.NonPublic | BindingFlags.Instance));

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(BirdTutorialGui).GetField("infoWidth", BindingFlags.NonPublic | BindingFlags.Instance));

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(BirdTutorialGui).GetField("infoHeight", BindingFlags.NonPublic | BindingFlags.Instance));

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(BirdTutorialGui).GetField("lineColor", BindingFlags.NonPublic | BindingFlags.Instance));

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(BirdTutorialGui).GetField("bgColor", BindingFlags.NonPublic | BindingFlags.Instance));

                cursor.EmitDelegate<Func<int, BirdTutorialGui, float, float, float, Color, Color, int>>((orig, self, controlsWidth, infoWidth, infoHeight, lineColor, bgColor) => {
                    if (self.Entity is CustomTutorialWithNoBird tutorial && tutorial.direction != Direction.Down) {
                        Vector2 position = (self.Entity.Position + self.Position - self.SceneAs<Level>().Camera.Position.Floor()) * 6f;
                        float width = (Math.Max(controlsWidth, infoWidth) + 64f) * self.Scale;
                        float height = infoHeight + ActiveFont.LineHeight + 32f;
                        float left = position.X - width / 2f;
                        float top = position.Y - height - 32f;

                        // draw a replacement pointer if the direction is Up, Left or Right
                        for (int i = 0; i <= 36; i++) {
                            float bubbleWidth = (73 - i * 2) * self.Scale;

                            switch (tutorial.direction) {
                                case Direction.Up:
                                    Draw.Rect(position.X - bubbleWidth / 2f, top - i - 1, bubbleWidth, 1f, lineColor);
                                    if (bubbleWidth > 12f) {
                                        Draw.Rect(position.X - bubbleWidth / 2f + 6f, top - i, bubbleWidth - 12f, 1f, bgColor);
                                    }
                                    break;
                                case Direction.Left:
                                    Draw.Rect(left - i - 1, top + height / 2f - bubbleWidth / 2f, 1f, bubbleWidth, lineColor);
                                    if (bubbleWidth > 12f) {
                                        Draw.Rect(left - i, top + height / 2f - bubbleWidth / 2f + 6f, 1f, bubbleWidth - 12f, bgColor);
                                    }
                                    break;
                                case Direction.Right:
                                    Draw.Rect(left + width + i, top + height / 2f - bubbleWidth / 2f, 1f, bubbleWidth, lineColor);
                                    if (bubbleWidth > 12f) {
                                        Draw.Rect(left + width + i, top + height / 2f - bubbleWidth / 2f + 6f, 1f, bubbleWidth - 12f, bgColor);
                                    }
                                    break;
                            }
                        }

                        // remove the vanilla pointer by breaking the for loop drawing it (either we just drew the pointer ourselves, or we want no pointer).
                        return -1;
                    }
                    return orig;
                });
            }
        }
    }
}
