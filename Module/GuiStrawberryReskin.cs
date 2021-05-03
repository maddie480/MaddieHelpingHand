using Monocle;
using MonoMod.Cil;
using System;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Module {
    static class GuiStrawberryReskin {
        private static bool isFileSelect;

        public static void Load() {
            IL.Celeste.StrawberriesCounter.Render += modStrawberrySkin;
            IL.Celeste.OuiChapterPanel.DrawCheckpoint += modStrawberrySkin;
            On.Celeste.OuiFileSelect.Enter += onFileSelectEnter;
            On.Celeste.OuiFileSelect.Leave += onFileSelectLeave;
        }

        public static void Unload() {
            IL.Celeste.StrawberriesCounter.Render -= modStrawberrySkin;
            IL.Celeste.OuiChapterPanel.DrawCheckpoint -= modStrawberrySkin;
            On.Celeste.OuiFileSelect.Enter -= onFileSelectEnter;
            On.Celeste.OuiFileSelect.Leave -= onFileSelectLeave;
        }

        private static void modStrawberrySkin(ILContext il) {
            ILCursor iLCursor = new ILCursor(il);
            while (iLCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("collectables/strawberry"))) {
                Logger.Log("MaxHelpingHand/GuiStrawberryReskin", $"Changing strawberry icon w/ custom one at {iLCursor.Index} in IL for {iLCursor.Method.FullName}");
                iLCursor.EmitDelegate<Func<string, string>>(orig => {
                    if (isFileSelect) {
                        return orig;
                    }

                    string levelSetName = (Engine.Scene as Overworld)?.GetUI<OuiChapterPanel>()?.Area.GetLevelSet();
                    if (levelSetName == null) {
                        levelSetName = (Engine.Scene as Level)?.Session?.Area.GetLevelSet();
                    }
                    if (levelSetName != null && GFX.Gui.Has($"MaxHelpingHand/{levelSetName}/strawberry")) {
                        return $"MaxHelpingHand/{levelSetName}/strawberry";
                    }
                    return orig;
                });
            }
        }

        private static IEnumerator onFileSelectEnter(On.Celeste.OuiFileSelect.orig_Enter orig, OuiFileSelect self, Oui from) {
            isFileSelect = true;
            return orig(self, from);
        }

        private static IEnumerator onFileSelectLeave(On.Celeste.OuiFileSelect.orig_Leave orig, OuiFileSelect self, Oui next) {
            yield return new SwapImmediately(orig(self, next));
            isFileSelect = false;
        }
    }
}
