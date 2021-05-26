using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/CustomSandwichLavaSettingsTrigger")]
    public class CustomSandwichLavaSettingsTrigger : Trigger {

        private bool onlyOnce;
        private CustomSandwichLava.DirectionMode direction;
        private float speed;

        public CustomSandwichLavaSettingsTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            onlyOnce = data.Bool("onlyOnce");
            direction = data.Enum("direction", CustomSandwichLava.DirectionMode.CoreModeBased);
            speed = data.Float("speed", 20f);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            CustomSandwichLava target = SceneAs<Level>().Tracker.GetEntity<CustomSandwichLava>();
            if (target != null) {
                target.Direction = direction;
                target.Speed = speed;
            }

            if (onlyOnce) {
                RemoveSelf();
            }
        }
    }
}
