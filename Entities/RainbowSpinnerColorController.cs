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
    [CustomEntity("MaxHelpingHand/RainbowSpinnerColorController", "MaxHelpingHand/FlagRainbowSpinnerColorController")]
    [Tracked]
    public class RainbowSpinnerColorController : Entity {
        private static readonly FieldInfo spinnerColor = typeof(CrystalStaticSpinner).GetField("color", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo spinnerUpdateHue = typeof(CrystalStaticSpinner).GetMethod("UpdateHue", BindingFlags.NonPublic | BindingFlags.Instance);

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
                && !self.Session.LevelData.Entities.Any(entity => entity.Name == "MaxHelpingHand/FlagRainbowSpinnerColorController" ||
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

                    { "flag", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.Flag },

                    { "colorsWithFlag", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.ColorsWithFlag },
                    { "gradientSizeWithFlag", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.GradientSizeWithFlag },
                    { "loopColorsWithFlag", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.LoopColorsWithFlag },
                    { "centerXWithFlag", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.CenterWithFlag.X },
                    { "centerYWithFlag", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.CenterWithFlag.Y },
                    { "gradientSpeedWithFlag", MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors.GradientSpeedWithFlag },

                    { "persistent", true }
                };

                self.Add(new RainbowSpinnerColorController(restoredData, Vector2.Zero));
                self.Entities.UpdateLists();
            }
        }


        private static bool rainbowSpinnerHueHooked = false;
        private static Hook jungleHelperHook;

        // the spinner controller on the current screen.
        internal static RainbowSpinnerColorController spinnerControllerOnScreen;

        // during transitions: the spinner controller on the next screen, and the progress between both screens.
        // transitionProgress = -1 means no transition is ongoing.
        internal static RainbowSpinnerColorController nextSpinnerController;
        internal static float transitionProgress = -1f;

        // the parameters for this spinner controller.
        // first of the tuple: if flag is disabled or undefined, second: if flag is enabled.
        private Tuple<Color[], Color[]> colors;
        private Tuple<float, float> gradientSize;
        private Tuple<bool, bool> loopColors;
        private Tuple<Vector2, Vector2> center;
        private Tuple<float, float> gradientSpeed;
        private bool persistent;

        private string flag;
        private bool flagLatestState;

        // the state that will be saved in session if this controller is added to the level
        private MaxHelpingHandSession.RainbowSpinnerColorState sessionState;

        public RainbowSpinnerColorController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            gradientSize = new Tuple<float, float>(data.Float("gradientSize", 280), data.Float("gradientSizeWithFlag", 280));
            loopColors = new Tuple<bool, bool>(data.Bool("loopColors"), data.Bool("loopColorsWithFlag"));
            center = new Tuple<Vector2, Vector2>(new Vector2(data.Float("centerX", 0), data.Float("centerY", 0)), new Vector2(data.Float("centerXWithFlag", 0), data.Float("centerYWithFlag", 0)));
            gradientSpeed = new Tuple<float, float>(data.Float("gradientSpeed", 50f), data.Float("gradientSpeedWithFlag", 50f));

            // convert the color list to Color objects
            string[] colorsAsStrings = data.Attr("colors", "89E5AE,88E0E0,87A9DD,9887DB,D088E2").Split(',');
            List<Color> colorsWithoutFlag = new List<Color>();
            for (int i = 0; i < colorsAsStrings.Length; i++) {
                colorsWithoutFlag.Add(Calc.HexToColor(colorsAsStrings[i]));
            }
            // if looping colors, add the first color again to the end of the list.
            if (loopColors.Item1) {
                colorsWithoutFlag.Add(colorsWithoutFlag[0]);
            }

            // do the same but for the flag colors
            colorsAsStrings = data.Attr("colorsWithFlag", "89E5AE,88E0E0,87A9DD,9887DB,D088E2").Split(',');
            List<Color> colorsWithFlag = new List<Color>();
            for (int i = 0; i < colorsAsStrings.Length; i++) {
                colorsWithFlag.Add(Calc.HexToColor(colorsAsStrings[i]));
            }
            if (loopColors.Item2) {
                colorsWithFlag.Add(colorsWithFlag[0]);
            }

            colors = new Tuple<Color[], Color[]>(colorsWithoutFlag.ToArray(), colorsWithFlag.ToArray());
            persistent = data.Bool("persistent");

            flag = data.Attr("flag");

            Add(new TransitionListener {
                OnIn = progress => transitionProgress = progress,
                OnOut = progress => transitionProgress = progress,
                OnInBegin = () => transitionProgress = 0f,
                OnInEnd = () => transitionProgress = -1f
            });

            // prepare the object that will be saved in session
            if (persistent) {
                sessionState = new MaxHelpingHandSession.RainbowSpinnerColorState() {
                    Colors = data.Attr("colors", "89E5AE,88E0E0,87A9DD,9887DB,D088E2"),
                    GradientSize = gradientSize.Item1,
                    LoopColors = loopColors.Item1,
                    Center = center.Item1,
                    GradientSpeed = gradientSpeed.Item1,

                    Flag = flag,

                    ColorsWithFlag = data.Attr("colorsWithFlag", "89E5AE,88E0E0,87A9DD,9887DB,D088E2"),
                    GradientSizeWithFlag = gradientSize.Item2,
                    LoopColorsWithFlag = loopColors.Item2,
                    CenterWithFlag = center.Item2,
                    GradientSpeedWithFlag = gradientSpeed.Item2
                };
            } else {
                sessionState = null;
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            ApplyToSession();
        }

        internal void ApplyToSession() {
            MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors = sessionState;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            flagLatestState = !string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag);

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

            // updating the spinner hue usually occurs on every spinner cycle (0.08 seconds).
            // all spinners have the cycle randomly offset, so they don't get the new colors at the same time,
            // making for a weird visual effect.
            // so we want to update the hue of **all** spinners forcibly when the flag is toggled.
            bool flagState = !string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag);
            if (flagState != flagLatestState) {
                flagLatestState = flagState;

                object[] noParameters = new object[0];
                foreach (CrystalStaticSpinner speen in Scene.Tracker.GetEntities<CrystalStaticSpinner>().Cast<CrystalStaticSpinner>()) {
                    // run UpdateHue on all rainbow spinners through reflection (both spinner.color and spinner.UpdateHue are private :a:)
                    if ((CrystalColor) spinnerColor.GetValue(speen) == CrystalColor.Rainbow) {
                        spinnerUpdateHue.Invoke(speen, noParameters);
                    }
                }
            }
        }

        private T selectFromFlag<T>(Tuple<T, T> setting) {
            if (string.IsNullOrEmpty(flag) || !SceneAs<Level>().Session.GetFlag(flag)) {
                return setting.Item1;
            }
            return setting.Item2;
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
                    return getModHue(spinnerControllerOnScreen.selectFromFlag(spinnerControllerOnScreen.colors),
                        spinnerControllerOnScreen.selectFromFlag(spinnerControllerOnScreen.gradientSize),
                        self.Scene, position,
                        spinnerControllerOnScreen.selectFromFlag(spinnerControllerOnScreen.loopColors),
                        spinnerControllerOnScreen.selectFromFlag(spinnerControllerOnScreen.center),
                        spinnerControllerOnScreen.selectFromFlag(spinnerControllerOnScreen.gradientSpeed));
                } else {
                    return origMethod();
                }
            } else {
                // get the spinner color in the room we're coming from.
                Color fromRoomColor;
                if (spinnerControllerOnScreen != null) {
                    fromRoomColor = getModHue(spinnerControllerOnScreen.selectFromFlag(spinnerControllerOnScreen.colors),
                        spinnerControllerOnScreen.selectFromFlag(spinnerControllerOnScreen.gradientSize),
                        self.Scene, position,
                        spinnerControllerOnScreen.selectFromFlag(spinnerControllerOnScreen.loopColors),
                        spinnerControllerOnScreen.selectFromFlag(spinnerControllerOnScreen.center),
                        spinnerControllerOnScreen.selectFromFlag(spinnerControllerOnScreen.gradientSpeed));
                } else {
                    fromRoomColor = origMethod();
                }

                // get the spinner color in the room we're going to.
                Color toRoomColor;
                if (nextSpinnerController != null) {
                    toRoomColor = getModHue(nextSpinnerController.selectFromFlag(nextSpinnerController.colors),
                        nextSpinnerController.selectFromFlag(nextSpinnerController.gradientSize),
                        self.Scene, position,
                        nextSpinnerController.selectFromFlag(nextSpinnerController.loopColors),
                        nextSpinnerController.selectFromFlag(nextSpinnerController.center),
                        nextSpinnerController.selectFromFlag(nextSpinnerController.gradientSpeed));
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
