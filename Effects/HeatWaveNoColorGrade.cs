using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    /// <summary>
    /// A heatwave effect that does not affect the colorgrade.
    /// </summary>
    class HeatWaveNoColorGrade : HeatWave {
        private DynData<HeatWave> self = new DynData<HeatWave>();
        private bool controlColorGradeWhenActive;

        public HeatWaveNoColorGrade(bool controlColorGradeWhenActive) : base() {
            self = new DynData<HeatWave>(this);
            this.controlColorGradeWhenActive = controlColorGradeWhenActive;
        }

        public override void Update(Scene scene) {
            Level level = scene as Level;
            bool show = (IsVisible(level) && level.CoreMode != Session.CoreModes.None);

            if (!show || !controlColorGradeWhenActive) {
                // if not fading out, the heatwave is invisible, so don't even bother updating it.
                if (show || self.Get<float>("fade") > 0) {
                    // be sure to lock color grading to prevent it from becoming "none".
                    DynData<Level> levelData = new DynData<Level>(level);

                    string lastColorGrade = levelData.Get<string>("lastColorGrade");
                    float colorGradeEase = levelData.Get<float>("colorGradeEase");
                    float colorGradeEaseSpeed = levelData.Get<float>("colorGradeEaseSpeed");
                    string colorGrade = level.Session.ColorGrade;

                    base.Update(scene);

                    levelData["lastColorGrade"] = lastColorGrade;
                    levelData["colorGradeEase"] = colorGradeEase;
                    levelData["colorGradeEaseSpeed"] = colorGradeEaseSpeed;
                    level.Session.ColorGrade = colorGrade;

                    if (self.Get<float>("heat") <= 0) {
                        // the heat hit 0, we should now restore the water sine direction
                        // ... because if we don't, waterfalls will flow backwards
                        Distort.WaterSineDirection = 1f;
                    }
                }
            } else {
                // heat wave is visible and we want it to control the color grade when active: update as usual.
                base.Update(scene);
            }
        }
    }
}
