using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// A controller allowing for customization of rainbow spinner colors.
    /// </summary>
    [CustomEntity("MaxHelpingHand/RainbowSpinnerColorController")]
    class RainbowSpinnerColorController : Entity {
        public static void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
        }

        private static void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors != null
                && self.Session.LevelData != null // happens if we are loading a save in a room that got deleted
                && !self.Session.LevelData.Entities.Any(entity =>
                    entity.Name == "MaxHelpingHand/RainbowSpinnerColorController" || entity.Name == "MaxHelpingHand/RainbowSpinnerColorControllerDisabler")) {

                // we have spinner colors in session, and are entering a room with no controller: spawn one.
                EntityData restoredData = new EntityData();
                restoredData.Values = new Dictionary<string, object>() {
                    { "colors", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.Colors },
                    { "gradientSize", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.GradientSize },
                    { "loopColors", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.LoopColors },
                    { "centerX", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.Center.X },
                    { "centerY", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.Center.Y },
                    { "gradientSpeed", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.GradientSpeed },
                    { "persistent", true }
                };

                self.Add(new RainbowSpinnerColorController(restoredData, Vector2.Zero));
                self.Entities.UpdateLists();
            }
        }


        private static bool rainbowSpinnerHueHooked = false;
        private static Hook jungleHelperHook;

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
        private Vector2 center;
        private float gradientSpeed;
        private bool persistent;

        public RainbowSpinnerColorController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            // convert the color list to Color objects
            string[] colorsAsStrings = data.Attr("colors", "89E5AE,88E0E0,87A9DD,9887DB,D088E2").Split(',');
            colors = new Color[colorsAsStrings.Length];
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = Calc.HexToColor(colorsAsStrings[i]);
            }

            gradientSize = data.Float("gradientSize", 280);
            loopColors = data.Bool("loopColors");
            center = new Vector2(data.Float("centerX", 0), data.Float("centerY", 0));
            gradientSpeed = data.Float("gradientSpeed", 50f);
            persistent = data.Bool("persistent");

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

            // update session
            if (persistent) {
                MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors = new MaxHelpingHandSession.RainbowSpinnerColorState() {
                    Colors = data.Attr("colors", "89E5AE,88E0E0,87A9DD,9887DB,D088E2"),
                    GradientSize = gradientSize,
                    LoopColors = loopColors,
                    Center = center,
                    GradientSpeed = gradientSpeed
                };
            } else {
                MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors = null;
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // this is the controller for the next screen.
            nextSpinnerController = this;

            // enable the hook on rainbow spinner hue.
            if (!rainbowSpinnerHueHooked) {
                On.Celeste.CrystalStaticSpinner.GetHue += getRainbowSpinnerHue;
                hookJungleHelper();
                rainbowSpinnerHueHooked = true;
            }
        }

        private void hookJungleHelper() {
            if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "JungleHelper", Version = new Version(1, 0, 0) })) {
                // we want to hook Color Celeste.Mod.JungleHelper.Components.RainbowDecalComponent.getHue(Vector2) in Jungle Helper.
                jungleHelperHook = new Hook(Everest.Modules.First(mod => mod.GetType().ToString() == "Celeste.Mod.JungleHelper.JungleHelperModule")
                    .GetType().Assembly.GetType("Celeste.Mod.JungleHelper.Components.RainbowDecalComponent").GetMethod("getHue", BindingFlags.NonPublic | BindingFlags.Instance),
                    typeof(RainbowSpinnerColorController).GetMethod("getRainbowDecalComponentHue", BindingFlags.NonPublic | BindingFlags.Static));
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
                jungleHelperHook?.Dispose();
                jungleHelperHook = null;
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
                jungleHelperHook?.Dispose();
                jungleHelperHook = null;
                rainbowSpinnerHueHooked = false;
            };
        }

        public override void Update() {
            base.Update();

            if (transitionProgress == -1f && spinnerControllerOnScreen == null) {
                // no transition is ongoing.
                // if only nextSpinnerController is defined, move it into spinnerControllerOnScreen.
                spinnerControllerOnScreen = nextSpinnerController;
                nextSpinnerController = null;
            }
        }

        private static Color getRainbowSpinnerHue(On.Celeste.CrystalStaticSpinner.orig_GetHue orig, CrystalStaticSpinner self, Vector2 position) {
            return getEntityHue(() => orig(self, position), self, position);
        }

        private static Color getRainbowDecalComponentHue(Func<Component, Vector2, Color> orig, Component self, Vector2 position) {
            return getEntityHue(() => orig(self, position), self.Entity, position);
        }

        private static Color getEntityHue(Func<Color> origMethod, Entity self, Vector2 position) {
            if (transitionProgress == -1f) {
                if (spinnerControllerOnScreen == null) {
                    // no transition is ongoing.
                    // if only nextSpinnerController is defined, move it into spinnerControllerOnScreen.
                    spinnerControllerOnScreen = nextSpinnerController;
                    nextSpinnerController = null;
                }

                if (spinnerControllerOnScreen != null) {
                    return getModHue(spinnerControllerOnScreen.colors, spinnerControllerOnScreen.gradientSize, self.Scene, position,
                        spinnerControllerOnScreen.loopColors, spinnerControllerOnScreen.center, spinnerControllerOnScreen.gradientSpeed);
                } else {
                    return origMethod();
                }
            } else {
                // get the spinner color in the room we're coming from.
                Color fromRoomColor;
                if (spinnerControllerOnScreen != null) {
                    fromRoomColor = getModHue(spinnerControllerOnScreen.colors, spinnerControllerOnScreen.gradientSize, self.Scene, position,
                        spinnerControllerOnScreen.loopColors, spinnerControllerOnScreen.center, spinnerControllerOnScreen.gradientSpeed);
                } else {
                    fromRoomColor = origMethod();
                }

                // get the spinner color in the room we're going to.
                Color toRoomColor;
                if (nextSpinnerController != null) {
                    toRoomColor = getModHue(nextSpinnerController.colors, nextSpinnerController.gradientSize, self.Scene, position,
                        nextSpinnerController.loopColors, nextSpinnerController.center, nextSpinnerController.gradientSpeed);
                } else {
                    toRoomColor = origMethod();
                }

                // transition smoothly between both.
                return Color.Lerp(fromRoomColor, toRoomColor, transitionProgress);
            }
        }

        internal static Color getModHue(Color[] colors, float gradientSize, Scene scene, Vector2 position, bool loopColors, Vector2 center, float gradientSpeed) {
            if (colors.Length == 1) {
                // edge case: there is 1 color, just return it!
                return colors[0];
            }

            float progress = (position - center).Length() + scene.TimeActive * gradientSpeed;
            while (progress < 0) {
                progress += gradientSize;
            }
            progress = progress % gradientSize / gradientSize;
            if (!loopColors) {
                progress = Calc.YoYo(progress);
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
