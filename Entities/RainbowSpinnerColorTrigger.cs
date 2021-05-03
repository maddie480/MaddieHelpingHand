using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/RainbowSpinnerColorTrigger")]
    class RainbowSpinnerColorTrigger : Trigger {
        private RainbowSpinnerColorController controller;

        public RainbowSpinnerColorTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            controller = new RainbowSpinnerColorController(data, offset);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            // remove the current controller from the room
            RainbowSpinnerColorController existingController = Scene.Tracker.GetEntity<RainbowSpinnerColorController>();
            if (existingController != null) {
                Scene.Remove(existingController);
            }

            // and add ours instead
            Scene.Add(controller);
        }
    }
}
