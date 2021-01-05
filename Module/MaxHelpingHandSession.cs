using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandSession : EverestModuleSession {
        public class RainbowSpinnerColorState {
            public string Colors { get; set; }
            public float GradientSize { get; set; }
            public bool LoopColors { get; set; }
            public Vector2 Center { get; set; } = new Vector2(0, 0);
            public float GradientSpeed { get; set; } = 50f;
        }

        public class SeekerBarrierColorState {
            public string Color { get; set; }
            public string ParticleColor { get; set; }
            public float Transparency { get; set; }
            public float ParticleTransparency { get; set; }
        }

        public class MultiRoomStrawberrySeedInfo {
            public int Index { get; set; }
            public EntityID BerryID { get; set; }
            public string Sprite { get; set; }
        }

        public class SpeedBasedMusicParamInfo {
            public float MinimumSpeed { get; set; }
            public float MaximumSpeed { get; set; }
            public float MinimumParamValue { get; set; }
            public float MaximumParamValue { get; set; }
        }

        public RainbowSpinnerColorState RainbowSpinnerCurrentColors { get; set; } = null;
        public SeekerBarrierColorState SeekerBarrierCurrentColors { get; set; } = null;

        public string GradientDustImagePath { get; set; } = null;
        public float GradientDustScrollSpeed { get; set; } = 0f;
        public float? CameraCatchupSpeed { get; set; } = null;
        public bool MadelineIsSilhouette { get; set; } = false;

        public List<MultiRoomStrawberrySeedInfo> CollectedMultiRoomStrawberrySeeds { get; set; } = new List<MultiRoomStrawberrySeedInfo>();

        public Dictionary<string, SpeedBasedMusicParamInfo> ActiveSpeedBasedMusicParams = new Dictionary<string, SpeedBasedMusicParamInfo>();
    }
}
