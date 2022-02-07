using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/SetBloomStrengthTrigger")]
    public class SetBloomStrengthTrigger : Trigger {
        public static void Load() {
            On.Celeste.LevelLoader.StartLevel += onLevelStart;
        }

        public static void Unload() {
            On.Celeste.LevelLoader.StartLevel -= onLevelStart;
        }

        private static void onLevelStart(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self) {
            // restore bloom strength upon reentering the level
            if (MaxHelpingHandModule.Instance.Session.BloomStregth.HasValue) {
                self.Level.Bloom.Strength = MaxHelpingHandModule.Instance.Session.BloomStregth.Value;
            }

            orig(self);
        }


        private float value;

        public SetBloomStrengthTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            value = data.Float("value");
        }

        public override void OnEnter(Player player) {
            Level level = Scene as Level;

            level.Bloom.Strength = value;
            MaxHelpingHandModule.Instance.Session.BloomStregth = value;
        }
    }
}
