using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Module {
    // This hook makes speech bubbles adapt their position on the screen if a Change Camera Angle Trigger from AvBdayHelper is used.
    public static class AvBdaySpeechBubbleFixup {
        private static bool activated = false;

        public static void LoadMods() {
            if (!activated && Everest.Loader.DependencyLoaded(new EverestModuleMetadata() { Name = "AvBdayHelper2021", Version = new Version(1, 0, 2) })) {
                IL.Celeste.TalkComponent.TalkComponentUI.Render += onTalkComponentUIRender;
                activated = true;
            }
        }

        public static void Unload() {
            if (activated) {
                IL.Celeste.TalkComponent.TalkComponentUI.Render -= onTalkComponentUIRender;
                activated = false;
            }
        }

        private static void onTalkComponentUIRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Vector2>("op_Subtraction"))) {
                Logger.Log("MaxHelpingHand/AvBdaySpeechBubbleFixup", $"Hooking entity position to reposition bubble at {cursor.Index} in IL for TalkComponentUI.Render");

                cursor.EmitDelegate<Func<Vector2, Vector2>>(orig => {
                    // we only have work to do if the camera has any form of angle.
                    Level level = Engine.Scene as Level;
                    if (level == null || level.Camera.Angle == 0f) {
                        return orig;
                    }

                    // rotate the relative position to the screen!
                    return orig.Rotate(level.Camera.Angle);
                });
            }
        }
    }
}
