using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/RainbowSpinnerColorAreaController")]
    [Tracked]
    class RainbowSpinnerColorAreaController : Entity {
        private static bool rainbowSpinnerHueHooked = false;
        private static Hook jungleHelperHook;

        // the parameters for this spinner controller.
        private Color[] colors;
        private float gradientSize;
        private bool loopColors;
        private Vector2 center;
        private float gradientSpeed;

        public RainbowSpinnerColorAreaController(EntityData data, Vector2 offset) : base(data.Position + offset) {
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

            // make this controller collidable.
            Collider = new Hitbox(data.Width, data.Height);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // enable the hook on rainbow spinner hue.
            if (!rainbowSpinnerHueHooked) {
                using (new DetourContext { After = { "*" } }) { // ensure we override rainbow spinner color controllers
                    On.Celeste.CrystalStaticSpinner.GetHue += getRainbowSpinnerHue;
                    hookJungleHelper();
                    rainbowSpinnerHueHooked = true;
                }
            }
        }

        private void hookJungleHelper() {
            if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "JungleHelper", Version = new Version(1, 0, 0) })) {
                // we want to hook Color Celeste.Mod.JungleHelper.Components.RainbowDecalComponent.getHue(Vector2) in Jungle Helper.
                jungleHelperHook = new Hook(Everest.Modules.First(mod => mod.GetType().ToString() == "Celeste.Mod.JungleHelper.JungleHelperModule")
                    .GetType().Assembly.GetType("Celeste.Mod.JungleHelper.Components.RainbowDecalComponent").GetMethod("getHue", BindingFlags.NonPublic | BindingFlags.Instance),
                    typeof(RainbowSpinnerColorAreaController).GetMethod("getRainbowDecalComponentHue", BindingFlags.NonPublic | BindingFlags.Static));
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            // if this controller was the last in the scene, disable the hook on rainbow spinner hue.
            if (rainbowSpinnerHueHooked && scene.Tracker.CountEntities<RainbowSpinnerColorAreaController>() <= 1) {
                On.Celeste.CrystalStaticSpinner.GetHue -= getRainbowSpinnerHue;
                jungleHelperHook?.Dispose();
                jungleHelperHook = null;
                rainbowSpinnerHueHooked = false;
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            // leaving level; disable the hook on rainbow spinner hue.
            if (rainbowSpinnerHueHooked) {
                On.Celeste.CrystalStaticSpinner.GetHue -= getRainbowSpinnerHue;
                jungleHelperHook?.Dispose();
                jungleHelperHook = null;
                rainbowSpinnerHueHooked = false;
            };
        }

        private static Color getRainbowSpinnerHue(On.Celeste.CrystalStaticSpinner.orig_GetHue orig, CrystalStaticSpinner self, Vector2 position) {
            RainbowSpinnerColorAreaController controller = self.CollideFirst<RainbowSpinnerColorAreaController>(position);
            if (controller != null) {
                // apply the color from the controller we are in.
                return RainbowSpinnerColorController.getModHue(controller.colors, controller.gradientSize, self.Scene, position, controller.loopColors, controller.center, controller.gradientSpeed);
            } else {
                // we are not in a controller; apply the vanilla color.
                return orig(self, position);
            }
        }

        private static Color getRainbowDecalComponentHue(Func<Component, Vector2, Color> orig, Component self, Vector2 position) {
            foreach (RainbowSpinnerColorAreaController controller in self.Scene.Tracker.GetEntities<RainbowSpinnerColorAreaController>()) {
                if (controller.Collider.Collide(position)) {
                    return RainbowSpinnerColorController.getModHue(controller.colors, controller.gradientSize, self.Scene, position, controller.loopColors, controller.center, controller.gradientSpeed);
                }
            }
            return orig(self, position);
        }
    }
}
