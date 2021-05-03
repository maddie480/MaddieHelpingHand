using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/RainbowSpinnerColorFadeTrigger")]
    class RainbowSpinnerColorFadeTrigger : Trigger {
        private RainbowSpinnerColorController controllerA;
        private RainbowSpinnerColorController controllerB;

        private PositionModes positionMode;

        public RainbowSpinnerColorFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            // instantiate the controllers.
            EntityData dataA = new EntityData();
            dataA.Values = new Dictionary<string, object>() {
                    { "colors", data.Attr("colorsA") },
                    { "gradientSize", data.Float("gradientSizeA") },
                    { "loopColors", data.Bool("loopColorsA") },
                    { "centerX", data.Float("centerXA") },
                    { "centerY", data.Float("centerYA") },
                    { "gradientSpeed", data.Float("gradientSpeedA") },
                    { "persistent", data.Bool("persistent") }
                };
            controllerA = new RainbowSpinnerColorController(dataA, Vector2.Zero);

            EntityData dataB = new EntityData();
            dataB.Values = new Dictionary<string, object>() {
                    { "colors", data.Attr("colorsB") },
                    { "gradientSize", data.Float("gradientSizeB") },
                    { "loopColors", data.Bool("loopColorsB") },
                    { "centerX", data.Float("centerXB") },
                    { "centerY", data.Float("centerYB") },
                    { "gradientSpeed", data.Float("gradientSpeedB") },
                    { "persistent", data.Bool("persistent") }
                };
            controllerB = new RainbowSpinnerColorController(dataB, Vector2.Zero);

            // initialize other parameters.
            positionMode = data.Enum<PositionModes>("direction");
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (Scene.Tracker.CountEntities<RainbowSpinnerColorController>() == 0) {
                // there is no controller, so some hooks will go missing if we start manipulating the controllers.
                // so inject one of them
                Scene.Add(controllerA);
            }
        }

        public override void OnStay(Player player) {
            base.OnStay(player);

            // we're reusing the mechanic that allows fading between rainbow spinner controller setups during transitions.
            RainbowSpinnerColorController.spinnerControllerOnScreen = controllerA;
            RainbowSpinnerColorController.nextSpinnerController = controllerB;
            RainbowSpinnerColorController.transitionProgress = GetPositionLerp(player, positionMode);
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            // discard whatever controller we currently have.
            Scene.Remove(Scene.Tracker.GetEntity<RainbowSpinnerColorController>());

            if (RainbowSpinnerColorController.transitionProgress > 0.5f) {
                // add controllerB to the scene.
                Scene.Add(RainbowSpinnerColorController.nextSpinnerController = controllerB);
            } else {
                // add controllerA to the scene.
                Scene.Add(RainbowSpinnerColorController.nextSpinnerController = controllerA);
            }

            // reset the transition variables to normal values.
            RainbowSpinnerColorController.spinnerControllerOnScreen = null;
            RainbowSpinnerColorController.transitionProgress = -1;
        }
    }
}
