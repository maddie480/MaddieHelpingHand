using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// A controller allowing for customization of rainbow spinner colors.
    /// </summary>
    [CustomEntity("MaxHelpingHand/RainbowSpinnerColorController")]
    class RainbowSpinnerColorController : Entity {
        private static bool rainbowSpinnerHueHooked = false;

        // the spinner controller on the current screen.
        private static RainbowSpinnerColorController spinnerControllerOnScreen;

        // during transitions: the spinner controller on the next screen, and the progress between both screens.
        // transitionProgress = -1 means no transition is ongoing.
        private static RainbowSpinnerColorController nextSpinnerController;
        private static float transitionProgress = -1f;

        // the parameters for this spinner controller.
        private Color[] colors;
        private float gradientSize;
        private bool loopColors;

        public RainbowSpinnerColorController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            // convert the color list to Color objects
            string[] colorsAsStrings = data.Attr("colors", "89E5AE,88E0E0,87A9DD,9887DB,D088E2").Split(',');
            colors = new Color[colorsAsStrings.Length];
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = Calc.HexToColor(colorsAsStrings[i]);
            }

            gradientSize = data.Float("gradientSize", 280);
            loopColors = data.Bool("loopColors");

            if (loopColors) {
                // let's cheat a bit and add A back at the end of the list
                Color[] newColors = new Color[colors.Length + 1];
                for (int i = 0; i < colors.Length; i++) {
                    newColors[i] = colors[i];
                }
                newColors[colors.Length] = colors[0];
                colors = newColors;
            }

            Add(new TransitionListener {
                OnIn = progress => transitionProgress = progress,
                OnOut = progress => transitionProgress = progress,
                OnInBegin = () => transitionProgress = 0f,
                OnInEnd = () => transitionProgress = -1f
            });
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // this is the controller for the next screen.
            nextSpinnerController = this;

            // enable the hook on rainbow spinner hue.
            if (!rainbowSpinnerHueHooked) {
                On.Celeste.CrystalStaticSpinner.GetHue += getRainbowSpinnerHue;
                rainbowSpinnerHueHooked = true;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            // the "current" spinner controller is now the one from the next screen.
            spinnerControllerOnScreen = nextSpinnerController;
            nextSpinnerController = null;

            // the transition (if any) is over.
            transitionProgress = -1f;

            // if there is none, clean up the hook on the spinner hue.
            if (spinnerControllerOnScreen == null && rainbowSpinnerHueHooked) {
                On.Celeste.CrystalStaticSpinner.GetHue -= getRainbowSpinnerHue;
                rainbowSpinnerHueHooked = false;
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            // leaving level: forget about all controllers and clean up the hook if present.
            spinnerControllerOnScreen = null;
            nextSpinnerController = null;
            if (rainbowSpinnerHueHooked) {
                On.Celeste.CrystalStaticSpinner.GetHue -= getRainbowSpinnerHue;
                rainbowSpinnerHueHooked = false;
            };
        }

        private static Color getRainbowSpinnerHue(On.Celeste.CrystalStaticSpinner.orig_GetHue orig, CrystalStaticSpinner self, Vector2 position) {
            if (transitionProgress == -1f) {
                // no transition is ongoing.
                // if only nextSpinnerController is defined, move it into spinnerControllerOnScreen.
                if (spinnerControllerOnScreen == null) {
                    spinnerControllerOnScreen = nextSpinnerController;
                    nextSpinnerController = null;
                }

                return getModHue(spinnerControllerOnScreen.colors, spinnerControllerOnScreen.gradientSize, self.Scene, self.Position, spinnerControllerOnScreen.loopColors);
            } else {
                // get the spinner color in the room we're coming from.
                Color fromRoomColor;
                if (spinnerControllerOnScreen != null) {
                    fromRoomColor = getModHue(spinnerControllerOnScreen.colors, spinnerControllerOnScreen.gradientSize, self.Scene, self.Position, spinnerControllerOnScreen.loopColors);
                } else {
                    fromRoomColor = orig(self, position);
                }

                // get the spinner color in the room we're going to.
                Color toRoomColor;
                if (nextSpinnerController != null) {
                    toRoomColor = getModHue(nextSpinnerController.colors, nextSpinnerController.gradientSize, self.Scene, self.Position, nextSpinnerController.loopColors);
                } else {
                    toRoomColor = orig(self, position);
                }

                // transition smoothly between both.
                return Color.Lerp(fromRoomColor, toRoomColor, transitionProgress);
            }
        }

        internal static Color getModHue(Color[] colors, float gradientSize, Scene scene, Vector2 position, bool loopColors) {
            float progress;
            if (loopColors) {
                progress = (position.Length() + scene.TimeActive * 50f) % gradientSize / gradientSize;
            } else {
                progress = Calc.YoYo((position.Length() + scene.TimeActive * 50f) % gradientSize / gradientSize);
            }

            if (progress == 1) {
                return colors[colors.Length - 1];
            }

            float globalProgress = (colors.Length - 1) * progress;
            int colorIndex = (int) globalProgress;
            float progressInIndex = globalProgress - colorIndex;
            return Color.Lerp(colors[colorIndex], colors[colorIndex + 1], progressInIndex);
        }
    }
}
