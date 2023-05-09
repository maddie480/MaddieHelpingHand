using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.MaxHelpingHand.Triggers;
using Monocle;
using MonoMod.ModInterop;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Module {
    // A class providing some methods that can be called from Lua cutscenes, by doing:
    // local luaCutscenesUtils = require("#Celeste.Mod.MaxHelpingHand.Module.LuaCutscenesUtils")
    // coroutine.yield(luaCutscenesUtils.SayWithDifferentFont(...))
    [ModExportName("MaddieHelpingHand")]
    public static class LuaCutscenesUtils {
        // Say a dialog string, like the "say" function from Lua Cutscenes, but with a different font.
        // What is expected here is the "face" attribute in the *.fnt file: <info face="Noto Sans CJK JP Medium" ...>
        // example: coroutine.yield(luaCutscenesUtils.SayWithDifferentFont("maddie480_testmap_comicsans", "maddie480_dialog"))
        public static IEnumerator SayWithDifferentFont(string font, string dialogId) {
            return ExtendedDialogCutsceneTrigger.ReplaceFancyTextFontFor(Textbox.Say(dialogId), font);
        }

        // Gives the player a choice, like the "choice" function from Lua Cutscenes, but with a different font.
        // What is expected here is the "face" attribute in the *.fnt file: <info face="Noto Sans CJK JP Medium" ...>
        // Get the choice of the user with GetChoice() afterwards.
        // example:
        // coroutine.yield(luaCutscenesUtils.ChoiceWithDifferentFont("maddie480_testmap_comicsans", "maddie480_choice1", "maddie480_choice2", "maddie480_choice3"))
        // local choiceIndex = luaCutscenesUtils.GetChoice()
        public static IEnumerator ChoiceWithDifferentFont(string font, params string[] options) {
            return ExtendedDialogCutsceneTrigger.ReplaceFancyTextFontFor(ChoicePrompt.Prompt(options), font);
        }

        public static int GetChoice() {
            return ChoicePrompt.Choice + 1;
        }

        // Triggers flag switch gate(s) manually based on their flag.
        // example: luaCutscenesUtils.TriggerFlagSwitchGate("flag_touch_switch")
        public static void TriggerFlagSwitchGate(string flag) {
            foreach (FlagSwitchGate gate in Engine.Scene.Tracker.GetEntities<FlagSwitchGate>()) {
                if (gate.Flag == flag) {
                    gate.Trigger();
                }
            }
        }
    }
}
