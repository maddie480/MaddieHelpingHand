using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /**
     * Spinner breaking balls for vanilla spinners, with an enumerated set of colors (including rainbow).
     */
    [CustomEntity("MaxHelpingHand/SpinnerBreakingBall")]
    public class SpinnerBreakingBallVanilla : SpinnerBreakingBallGeneric<CrystalStaticSpinner, CrystalColor> {
        private static MethodInfo crystalSpinnerGetHue = typeof(CrystalStaticSpinner).GetMethod("GetHue", BindingFlags.NonPublic | BindingFlags.Instance);

        private CrystalStaticSpinner rainbowSpinner;
        private bool rainbowTinting;

        public SpinnerBreakingBallVanilla(EntityData data, Vector2 offset, EntityID entityID)
            : base(data, offset, entityID, data.Enum("color", CrystalColor.Blue)) {

            rainbowTinting = data.Bool("rainbowTinting", defaultValue: true);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (color == CrystalColor.Rainbow && rainbowTinting) {
                // the GetHue method of crystal spinners require a spinner that is part of the scene.
                scene.Add(rainbowSpinner = new CrystalStaticSpinner(new Vector2(float.MinValue, float.MinValue), false, CrystalColor.Red));
                rainbowSpinner.AddTag(Tags.Persistent);
                rainbowSpinner.Visible = false;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            rainbowSpinner?.RemoveSelf();
        }

        public override void Update() {
            base.Update();

            if (rainbowSpinner != null) {
                // sync the ball's color with the color of the spinners around it
                sprite.Color = (Color) crystalSpinnerGetHue.Invoke(rainbowSpinner, new object[] { Position });
            }
        }

        protected override CrystalColor getColor(CrystalStaticSpinner spinner) {
            return new DynData<CrystalStaticSpinner>(spinner).Get<CrystalColor>("color");
        }

        protected override bool getAttachToSolid(CrystalStaticSpinner spinner) {
            return spinner.AttachToSolid;
        }

        protected override void destroySpinner(CrystalStaticSpinner spinner) {
            spinner.Destroy();
        }
    }
}
