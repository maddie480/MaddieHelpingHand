using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public static class CustomChapterNumber {
        public static void Load() {
            On.Celeste.OuiChapterPanel.Reset += onChapterPanelReset;
        }

        public static void Unload() {
            On.Celeste.OuiChapterPanel.Reset -= onChapterPanelReset;
        }

        private static void onChapterPanelReset(On.Celeste.OuiChapterPanel.orig_Reset orig, OuiChapterPanel self) {
            orig(self);

            string dialogId = ("maddiehelpinghand_chapternumber_" + self.Area.GetSID()).DialogKeyify();
            if (Dialog.Has(dialogId)) {
                new DynData<OuiChapterPanel>(self)["chapter"] = Dialog.Clean(dialogId);
            }
        }
    }
}
