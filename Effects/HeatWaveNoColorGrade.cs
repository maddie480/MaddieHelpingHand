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
        private bool controlColorGradeWhenActive;
        private bool renderParticles;

        public static void Load() {
            IL.Celeste.HeatWave.Render += modHeatWaveRender;
        }

        public static void Unload() {
            IL.Celeste.HeatWave.Render -= modHeatWaveRender;
        }

        public HeatWaveNoColorGrade(bool controlColorGradeWhenActive, bool renderParticles) : base() {
            this.controlColorGradeWhenActive = controlColorGradeWhenActive;
            this.renderParticles = renderParticles;
        }

        public override void Update(Scene scene) {
            Level level = scene as Level;
            bool show = (IsVisible(level) && level.CoreMode != Session.CoreModes.None);

            if (!show || !controlColorGradeWhenActive) {
                // if not fading out, the heatwave is invisible, so don't even bother updating it.
                if (show || fade > 0) {
                    // be sure to lock color grading to prevent it from becoming "none".
                    string lastColorGrade = level.lastColorGrade;
                    float colorGradeEase = level.colorGradeEase;
                    float colorGradeEaseSpeed = level.colorGradeEaseSpeed;
                    string colorGrade = level.Session.ColorGrade;

                    base.Update(scene);

                    level.lastColorGrade = lastColorGrade;
                    level.colorGradeEase = colorGradeEase;
                    level.colorGradeEaseSpeed = colorGradeEaseSpeed;
                    level.Session.ColorGrade = colorGrade;

                    if (heat <= 0) {
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
                cursor.EmitDelegate<Func<int, HeatWave, int>>(hideParticles);
            }
        }

        private static int hideParticles(int orig, HeatWave self) {
            if (self is HeatWaveNoColorGrade heatWave && !heatWave.renderParticles) {
                return 0; // there are no particles.
            }
            return orig;
        }
    }
}
