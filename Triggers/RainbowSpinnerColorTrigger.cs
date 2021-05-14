using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/RainbowSpinnerColorTrigger")]
    class RainbowSpinnerColorTrigger : Trigger {
        private RainbowSpinnerColorController controller;

        public RainbowSpinnerColorTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            controller = new RainbowSpinnerColorController(data, offset);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            RainbowSpinnerColorController existingController = Scene.Tracker.GetEntity<RainbowSpinnerColorController>();

            // if the current controller is not in the room, add it, and replace the current one if there is one.
            if (existingController != controller) {
                // remove the current controller from the room
                if (existingController != null) {
                    Scene.Remove(existingController);
                }

                // and add ours instead
                Scene.Add(controller);
            }
        }
    }
}
