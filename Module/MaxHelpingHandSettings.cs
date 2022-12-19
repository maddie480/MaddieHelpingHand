
namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandSettings : EverestModuleSettings {
        [DefaultButtonBinding(Microsoft.Xna.Framework.Input.Buttons.RightShoulder, Microsoft.Xna.Framework.Input.Keys.H)]
        public ButtonBinding ShowHints { get; set; }

        public bool DisableDialogueAutoSkip { get; set; } = false;
    }
}
