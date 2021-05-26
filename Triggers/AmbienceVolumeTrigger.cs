using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/AmbienceVolumeTrigger")]
    public class AmbienceVolumeTrigger : Trigger {
        public static void Load() {
            On.Celeste.Level.Update += onLevelUpdate;
        }

        public static void Unload() {
            On.Celeste.Level.Update -= onLevelUpdate;
        }

        private static void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            orig(self);

            float ambienceVolume = MaxHelpingHandModule.Instance.Session.AmbienceVolume;
            if (ambienceVolume < 1) {
                Audio.CurrentAmbienceEventInstance?.setVolume(ambienceVolume);
            }
        }

        private float from;
        private float to;
        private PositionModes positionMode;

        public AmbienceVolumeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            from = data.Float("from");
            to = data.Float("to");
            positionMode = data.Enum("direction", PositionModes.NoEffect);
        }

        public override void OnStay(Player player) {
            float ambienceVolume = Calc.ClampedMap(GetPositionLerp(player, positionMode), 0f, 1f, from, to);
            MaxHelpingHandModule.Instance.Session.AmbienceVolume = ambienceVolume;
            Audio.CurrentAmbienceEventInstance?.setVolume(ambienceVolume);
        }
    }
}
