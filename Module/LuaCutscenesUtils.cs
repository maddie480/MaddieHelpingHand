using Celeste.Mod.LuaCutscenes;
using Celeste.Mod.MaxHelpingHand.Triggers;
using Monocle;
using System.Collections;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Module {
    // A class providing some methods that can be called from Lua cutscenes, by doing:
    // local luaCutscenesUtils = require("#Celeste.Mod.MaxHelpingHand.Module.LuaCutscenesUtils")
    // coroutine.yield(luaCutscenesUtils.SayWithDifferentFont(...))
    public static class LuaCutscenesUtils {
        // Say a dialog string, like the "say" function from Lua Cutscenes, but with a different font.
        // What is expected here is the "face" attribute in the *.fnt file: <info face="Noto Sans CJK JP Medium" ...>
        // example: coroutine.yield(luaCutscenesUtils.SayWithDifferentFont("max480_testmap_comicsans", "max480_dialog"))
        public static IEnumerator SayWithDifferentFont(string font, string dialogId) {
            return ExtendedDialogCutsceneTrigger.LoadFontForDurationOfCoroutine(font,
                fontFace => ExtendedDialogCutsceneTrigger.SayWithDifferentFont(fontFace, dialogId));
        }

        // Gives the player a choice, like the "choice" function from Lua Cutscenes, but with a different font.
        // What is expected here is the "face" attribute in the *.fnt file: <info face="Noto Sans CJK JP Medium" ...>
        // Get the choice of the user with GetChoice() afterwards.
        // example:
        // coroutine.yield(luaCutscenesUtils.ChoiceWithDifferentFont("max480_testmap_comicsans", "max480_choice1", "max480_choice2", "max480_choice3"))
        // local choiceIndex = luaCutscenesUtils.GetChoice()
        public static IEnumerator ChoiceWithDifferentFont(string font, params string[] options) {
            return ExtendedDialogCutsceneTrigger.LoadFontForDurationOfCoroutine(font,
                fontFace => choiceWithDifferentFontCoroutine(fontFace, options));
        }

        public static int GetChoice() {
            return ChoicePrompt.Choice + 1;
        }

        private static IEnumerator choiceWithDifferentFontCoroutine(PixelFont font, string[] options) {
            IEnumerator say = ChoicePrompt.Prompt(options);

            if (font != null && say.MoveNext()) {
                // the MoveNext above added the options to the scene, this is time to swap the font!
                foreach (Option option in Engine.Scene.Entities.GetToAdd().OfType<Option>()) {
                    option.Text.Font = font;
                }

                // pass the value (probably null) that we consumed by calling MoveNext above.
                yield return say.Current;
            }

            // now just exhaust the rest of the coroutine.
            yield return new SwapImmediately(say);
        }
    }
}
