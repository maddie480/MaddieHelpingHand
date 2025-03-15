using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/RainbowSpinnerColorFadeTrigger")]
    public class RainbowSpinnerColorFadeTrigger : Trigger {
        private RainbowSpinnerColorController controllerA;
        private RainbowSpinnerColorController controllerB;

        private PositionModes positionMode;
        private bool diedInTrigger = false;

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

            if (player.Dead) {
                // player died in trigger, so leave the configuration as is until the screen fades out,
                // to avoid "snapping" to one of the controller configurations.
                diedInTrigger = true;
            } else {
                // player left the trigger.
                // choose the controller we want to keep: that's the one that is on the side the player left in.
                RainbowSpinnerColorController chosenController =
                    RainbowSpinnerColorController.transitionProgress > 0.5f ? controllerB : controllerA;
                RainbowSpinnerColorController.nextSpinnerController = chosenController;

                // if we need to, swap the controller currently in our scene with the Chosen One.
                RainbowSpinnerColorController currentController = Scene.Tracker.GetEntity<RainbowSpinnerColorController>();
                if (currentController != chosenController) {
                    Scene.Remove(currentController);
                    Scene.Add(chosenController);
                }

                // reset the transition variables to normal values.
                RainbowSpinnerColorController.spinnerControllerOnScreen = null;
                RainbowSpinnerColorController.transitionProgress = -1;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            if (diedInTrigger) {
                // the level is getting unloaded after the player died inside the trigger.
                // reset rainbow spinner controller configuration now.
                RainbowSpinnerColorController.spinnerControllerOnScreen = null;
                RainbowSpinnerColorController.nextSpinnerController = null;
                RainbowSpinnerColorController.transitionProgress = -1;
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            if (PlayerIsInside) {
                // player saved & quit inside the trigger, so... we need to do something about it.
                // let's save the configuration of the side of the trigger the player is closest to.
                if (RainbowSpinnerColorController.transitionProgress > 0.5f) {
                    controllerB.ApplyToSession();
                } else {
                    controllerA.ApplyToSession();
                }

                // also make sure to reset the transition progress.
                RainbowSpinnerColorController.transitionProgress = -1f;
            }
        }
    }
}
