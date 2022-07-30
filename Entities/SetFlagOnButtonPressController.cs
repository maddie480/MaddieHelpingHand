using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/SetFlagOnButtonPressController")]
    [Tracked]
    public class SetFlagOnButtonPressController : Entity {
        public static void Load() {
            On.Celeste.Input.Initialize += onInputInitialize;
        }

        public static void Unload() {
            On.Celeste.Input.Initialize -= onInputInitialize;
        }

        private static void onInputInitialize(On.Celeste.Input.orig_Initialize orig) {
            orig();

            // in case someone decided to change their bindings mid-game, we need to do reflection stuff again to get the new VirtualButton instance.
            if (Engine.Scene is Level) {
                foreach (SetFlagOnButtonPressController controller in Engine.Scene.Tracker.GetEntities<SetFlagOnButtonPressController>()) {
                    controller.resolveVirtualButton();
                }
            }
        }

        private readonly string buttonName;
        private readonly string flagName;
        private readonly bool inverted;
        private readonly bool toggleMode;
        private VirtualButton button;

        public SetFlagOnButtonPressController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            buttonName = data.Attr("button");
            flagName = data.Attr("flag");
            inverted = data.Bool("inverted");
            toggleMode = data.Bool("toggleMode");

            resolveVirtualButton();
        }

        private void resolveVirtualButton() {
            button = (VirtualButton) typeof(Input).GetField(buttonName).GetValue(null);
        }

        public override void Update() {
            base.Update();

            if (toggleMode) {
                // invert the flag whenever the button is pressed
                if (button.Pressed) {
                    Session session = SceneAs<Level>().Session;
                    session.SetFlag(flagName, !session.GetFlag(flagName));
                }
            } else {
                // enable the flag (or disable, depending on the inverted setting) whenever the button is held
                bool enableFlag = button.Check;

                if (inverted) {
                    enableFlag = !enableFlag;
                }

                SceneAs<Level>().Session.SetFlag(flagName, enableFlag);
            }
        }
    }
}
