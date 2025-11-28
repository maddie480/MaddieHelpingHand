using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/RainbowSpinnerColorAreaController", "MaxHelpingHand/FlagRainbowSpinnerColorAreaController")]
    [Tracked]
    public class RainbowSpinnerColorAreaController : Entity {
        private static readonly FieldInfo spinnerColor = typeof(CrystalStaticSpinner).GetField("color", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo spinnerUpdateHue = typeof(CrystalStaticSpinner).GetMethod("UpdateHue", BindingFlags.NonPublic | BindingFlags.Instance);

        private static bool rainbowSpinnerHueHookEnabled = false;
        private static Hook jungleHelperHook;

        public static void Load() {
            using (new DetourConfigContext(new DetourConfig("MaddieHelpingHand_AfterAll").WithPriority(int.MaxValue)).Use()) { // ensure we override rainbow spinner color controllers
                On.Celeste.CrystalStaticSpinner.GetHue += getRainbowSpinnerHue;
            }
        }

        public static void LoadMods() {
            using (new DetourConfigContext(new DetourConfig("MaddieHelpingHand_AfterAll").WithPriority(int.MaxValue)).Use()) { // ensure we override rainbow spinner color controllers
                if (jungleHelperHook == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "JungleHelper", Version = new Version(1, 0, 0) })) {
                    // we want to hook Color Celeste.Mod.JungleHelper.Components.RainbowDecalComponent.getHue(Vector2) in Jungle Helper.
                    jungleHelperHook = new Hook(Everest.Modules.First(mod => mod.GetType().ToString() == "Celeste.Mod.JungleHelper.JungleHelperModule")
                        .GetType().Assembly.GetType("Celeste.Mod.JungleHelper.Components.RainbowDecalComponent").GetMethod("getHue", BindingFlags.NonPublic | BindingFlags.Instance),
                        typeof(RainbowSpinnerColorAreaController).GetMethod("getRainbowDecalComponentHue", BindingFlags.NonPublic | BindingFlags.Static));
                }
            }
        }

        public static void Unload() {
            On.Celeste.CrystalStaticSpinner.GetHue -= getRainbowSpinnerHue;

            jungleHelperHook?.Dispose();
            jungleHelperHook = null;
        }

        // the parameters for this spinner controller.
        // first of the tuple: if flag is disabled or undefined, second: if flag is enabled.
        private Tuple<Color[], Color[]> colors;
        private Tuple<float, float> gradientSize;
        private Tuple<bool, bool> loopColors;
        private Tuple<Vector2, Vector2> center;
        private Tuple<float, float> gradientSpeed;

        private string flag;
        private bool flagLatestState;

        public RainbowSpinnerColorAreaController(EntityData data, Vector2 offset) : base(data.Position + offset) {
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

            flag = data.Attr("flag");

            // make this controller collidable.
            Collider = new Hitbox(data.Width, data.Height);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            flagLatestState = !string.IsNullOrEmpty(flag) && SceneAs<Level>().Session.GetFlag(flag);

            // enable the hook on rainbow spinner hue.
            rainbowSpinnerHueHookEnabled = true;
        }

        public override void Update() {
            base.Update();

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

        public override void Removed(Scene scene) {
            base.Removed(scene);

            // if this controller was the last in the scene, disable the hook on rainbow spinner hue.
            if (scene.Tracker.CountEntities<RainbowSpinnerColorAreaController>() <= 1) {
                rainbowSpinnerHueHookEnabled = false;
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            // leaving level; disable the hook on rainbow spinner hue.
            rainbowSpinnerHueHookEnabled = false;
        }

        private T selectFromFlag<T>(Tuple<T, T> setting) {
            if (string.IsNullOrEmpty(flag) || !SceneAs<Level>().Session.GetFlag(flag)) {
                return setting.Item1;
            }
            return setting.Item2;
        }

        private static Color getRainbowSpinnerHue(On.Celeste.CrystalStaticSpinner.orig_GetHue orig, CrystalStaticSpinner self, Vector2 position) {
            if (!rainbowSpinnerHueHookEnabled) return orig(self, position);

            RainbowSpinnerColorAreaController controller = self.CollideFirst<RainbowSpinnerColorAreaController>(position);
            if (controller != null) {
                // apply the color from the controller we are in.
                return RainbowSpinnerColorController.getModHue(controller.selectFromFlag(controller.colors),
                    controller.selectFromFlag(controller.gradientSize), self.Scene, position,
                    controller.selectFromFlag(controller.loopColors), controller.selectFromFlag(controller.center), controller.selectFromFlag(controller.gradientSpeed));
            } else {
                // we are not in a controller; apply the vanilla color.
                return orig(self, position);
            }
        }

        private static Color getRainbowDecalComponentHue(Func<Component, Vector2, Color> orig, Component self, Vector2 position) {
            if (!rainbowSpinnerHueHookEnabled) return orig(self, position);

            foreach (RainbowSpinnerColorAreaController controller in self.Scene.Tracker.GetEntities<RainbowSpinnerColorAreaController>()) {
                if (controller.Collider.Collide(position)) {
                    return RainbowSpinnerColorController.getModHue(controller.selectFromFlag(controller.colors),
                        controller.selectFromFlag(controller.gradientSize), self.Scene, position,
                        controller.selectFromFlag(controller.loopColors), controller.selectFromFlag(controller.center), controller.selectFromFlag(controller.gradientSpeed));
                }
            }
            return orig(self, position);
        }
    }
}
