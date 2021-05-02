using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/RainbowSpinnerColorFadeTrigger")]
    class RainbowSpinnerColorFadeTrigger : Trigger {
        private RainbowSpinnerColorController controllerA;
        private RainbowSpinnerColorController controllerB;
        private EntityData dataA;
        private EntityData dataB;

        private PositionModes positionMode;

        public RainbowSpinnerColorFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            // the RainbowSpinnerColorController has the side effect of wiping the session, so we want to keep it.
            MaxHelpingHandSession.RainbowSpinnerColorState backup = MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors;

            // instantiate the controllers.
            dataA = new EntityData();
            dataA.Values = new Dictionary<string, object>() {
                    { "colors", data.Attr("colorsA") },
                    { "gradientSize", data.Float("gradientSizeA") },
                    { "loopColors", data.Bool("loopColorsA") },
                    { "centerX", data.Float("centerXA") },
                    { "centerY", data.Float("centerYA") },
                    { "gradientSpeed", data.Float("gradientSpeedA") }
                };
            controllerA = new RainbowSpinnerColorController(dataA, Vector2.Zero);

            dataB = new EntityData();
            dataB.Values = new Dictionary<string, object>() {
                    { "colors", data.Attr("colorsB") },
                    { "gradientSize", data.Float("gradientSizeB") },
                    { "loopColors", data.Bool("loopColorsB") },
                    { "centerX", data.Float("centerXB") },
                    { "centerY", data.Float("centerYB") },
                    { "gradientSpeed", data.Float("gradientSpeedB") }
                };
            controllerB = new RainbowSpinnerColorController(dataB, Vector2.Zero);

            // restore the session.
            MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors = backup;

            // initialize other parameters.
            positionMode = data.Enum<PositionModes>("direction");

            dataA.Values["persistent"] = data.Bool("persistent");
            dataB.Values["persistent"] = data.Bool("persistent");
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

            // add the controller we want to keep.
            // note: rebuilding the controllers allows to apply the "persistent" parameter, which is applied in the constructor.
            if (RainbowSpinnerColorController.transitionProgress > 0.5f) {
                // add controllerB to the scene.
                Scene.Add(RainbowSpinnerColorController.nextSpinnerController = new RainbowSpinnerColorController(dataB, Vector2.Zero));
            } else {
                // add controllerA to the scene.
                Scene.Add(RainbowSpinnerColorController.nextSpinnerController = new RainbowSpinnerColorController(dataA, Vector2.Zero));
            }

            // reset the transition variables to normal values.
            RainbowSpinnerColorController.spinnerControllerOnScreen = null;
            RainbowSpinnerColorController.transitionProgress = -1;
        }
    }
}
