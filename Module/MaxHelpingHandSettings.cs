using Microsoft.Xna.Framework.Input;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandSettings : EverestModuleSettings {
        [DefaultButtonBinding(Buttons.RightShoulder, Keys.H)]
        public ButtonBinding ShowHints { get; set; }

        public bool DisableDialogueAutoSkip { get; set; } = false;
    }
}
