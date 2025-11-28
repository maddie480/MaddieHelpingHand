using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public static class GuiStrawberryReskin {
        private static bool isFileSelect;

        private static ILHook hookChapterPanelDrawCheckpoint = null;

        public static void Load() {
            IL.Celeste.StrawberriesCounter.Render += modStrawberrySkin;
            On.Celeste.OuiFileSelect.Enter += onFileSelectEnter;
            On.Celeste.OuiFileSelect.Leave += onFileSelectLeave;

            MethodInfo chapterPanelDrawCheckpoint = typeof(OuiChapterPanel).GetMethod("orig_DrawCheckpoint", BindingFlags.NonPublic | BindingFlags.Instance);
            if (chapterPanelDrawCheckpoint == null) {
                chapterPanelDrawCheckpoint = typeof(OuiChapterPanel).GetMethod("DrawCheckpoint", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            hookChapterPanelDrawCheckpoint = new ILHook(chapterPanelDrawCheckpoint, modStrawberrySkin);
        }

        public static void Unload() {
            IL.Celeste.StrawberriesCounter.Render -= modStrawberrySkin;
            On.Celeste.OuiFileSelect.Enter -= onFileSelectEnter;
            On.Celeste.OuiFileSelect.Leave -= onFileSelectLeave;

            hookChapterPanelDrawCheckpoint?.Dispose();
            hookChapterPanelDrawCheckpoint = null;
        }

        private static void modStrawberrySkin(ILContext il) {
            // replace the strawberry, then the golden berry icon.
            ILCursor iLCursor = new ILCursor(il);
            replaceStrawberrySprite(iLCursor, "strawberry", replaceStrawberrySprite);
            iLCursor.Index = 0;
            replaceStrawberrySprite(iLCursor, "goldberry", replaceGoldberrySprite);
        }

        private static void replaceStrawberrySprite(ILCursor iLCursor, string name, Func<string, string> replacer) {
            while (iLCursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr($"collectables/{name}"))) {
                Logger.Log("MaxHelpingHand/GuiStrawberryReskin", $"Changing {name} icon w/ custom one at {iLCursor.Index} in IL for {iLCursor.Method.FullName}");
                iLCursor.EmitDelegate<Func<string, string>>(replacer);
            }
        }

        private static string replaceStrawberrySprite(string orig) => replaceBerrySprite("strawberry", orig);
        private static string replaceGoldberrySprite(string orig) => replaceBerrySprite("goldberry", orig);

        private static string replaceBerrySprite(string name, string orig) {
            if (isFileSelect) {
                return orig;
            }

            // try figuring out the current area, and if we can't, bail out!
            AreaKey? area = (Engine.Scene as Overworld)?.GetUI<OuiChapterPanel>()?.Area;
            if (area == null) {
                area = (Engine.Scene as Level)?.Session?.Area;
            }
            if (area == null) {
                return orig;
            }

            if (GFX.Gui.Has($"MaxHelpingHand/{area.Value.SID}_{name}")) {
                return $"MaxHelpingHand/{area.Value.SID}_{name}";
            }
            if (GFX.Gui.Has($"MaxHelpingHand/{area.Value.LevelSet}/{name}")) {
                return $"MaxHelpingHand/{area.Value.LevelSet}/{name}";
            }
            return orig;
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
