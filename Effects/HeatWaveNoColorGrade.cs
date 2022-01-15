using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    /// <summary>
    /// A heatwave effect that does not affect the colorgrade.
    /// </summary>
    public class HeatWaveNoColorGrade : HeatWave {
        private DynData<HeatWave> self = new DynData<HeatWave>();
        private bool controlColorGradeWhenActive;
        private bool renderParticles;

        public static void Load() {
            IL.Celeste.HeatWave.Render += modHeatWaveRender;
        }

        public static void Unload() {
            IL.Celeste.HeatWave.Render -= modHeatWaveRender;
        }

        public HeatWaveNoColorGrade(bool controlColorGradeWhenActive, bool renderParticles) : base() {
            self = new DynData<HeatWave>(this);
            this.controlColorGradeWhenActive = controlColorGradeWhenActive;
            this.renderParticles = renderParticles;
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

        private static void modHeatWaveRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<HeatWave>("particles"), instr => instr.MatchLdlen())) {
                Logger.Log("MaxHelpingHand/HeatWaveNoColorGrade", $"Hiding particles at {cursor.Index} in IL for HeatWave.Render");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, HeatWave, int>>((orig, self) => {
                    if (self is HeatWaveNoColorGrade heatWave && !heatWave.renderParticles) {
                        return 0; // there are no particles.
                    }
                    return orig;
                });
            }
        }
    }
}
