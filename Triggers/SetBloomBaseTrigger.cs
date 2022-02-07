using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/SetBloomBaseTrigger")]
    public class SetBloomBaseTrigger : Trigger {
        private float value;

        public SetBloomBaseTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            value = data.Float("value");
        }

        public override void OnEnter(Player player) {
            Level level = Scene as Level;

            level.Bloom.Base = value;
            level.Session.BloomBaseAdd = value - AreaData.Get(level).BloomBase;
        }
    }
}
