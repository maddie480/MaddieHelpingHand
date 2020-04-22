using Celeste.Mod.MaxHelpingHand.Entities;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandModule : EverestModule {
        public override void Load() {
            TempleEyeTrackingMadeline.Load();
        }

        public override void Unload() {
            TempleEyeTrackingMadeline.Unload();
        }
    }
}
