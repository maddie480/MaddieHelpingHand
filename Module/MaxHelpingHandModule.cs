using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.MaxHelpingHand.Triggers;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandModule : EverestModule {
        public override void Load() {
            TempleEyeTrackingMadeline.Load();
            CameraCatchupSpeedTrigger.Load();
        }

        public override void Unload() {
            TempleEyeTrackingMadeline.Unload();
            CameraCatchupSpeedTrigger.Unload();
        }
    }
}
