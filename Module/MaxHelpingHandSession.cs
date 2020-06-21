
namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandSession : EverestModuleSession {
        public class RainbowSpinnerColorState {
            public string Colors { get; set; }
            public float GradientSize { get; set; }
            public bool LoopColors { get; set; }
        }

        public RainbowSpinnerColorState RainbowSpinnerCurrentColors { get; set; } = null;
    }
}
