using Microsoft.Xna.Framework;
using System;
using Monocle;
using MonoMod;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class SnowCustomColors : Snow {
        private Vector2 dir, perp;
        private float sineAmplitudeMult;

        public SnowCustomColors(Color[] colors, float speedMin, float speedMax, int particleCount, float angle, float sineAmplitudeMult) : base(false) {
            // redo the same operations as the vanilla constructor, but with our custom set of colors.
            this.colors = colors;
            blendedColors = new Color[colors.Length];

            // recreate the particles array with our custom count, colors, and min/max speeds
            particles = new Particle[particleCount];
            for (int i = 0; i < particles.Length; i++) {
                particles[i].Init(colors.Length, speedMin, speedMax);
            }

            dir = Calc.AngleToVector(angle, 1f);
            perp = dir.Perpendicular();
            this.sineAmplitudeMult = sineAmplitudeMult;
        }

        [MonoModLinkTo("Celeste.Backdrop", "System.Void Update(Monocle.Scene)")]
        private void base_Update(Scene scene) {
            throw new NotImplementedException("WTF? MonoModLinkTo is supposed to have relinked calls to this method!");
        }

        public override void Update(Scene scene) {
            base_Update(scene);

            if (scene is not Level level) {
                return;
            }

            visibleFade = Calc.Approach(visibleFade, IsVisible(level) ? 1f : 0f, Engine.DeltaTime / 0.5f);
            linearFade = 1f;
            if (FadeX != null) {
                linearFade *= FadeX.Value(level.Camera.X + level.Camera.Viewport.Width / 2f);
            }
            if (FadeY != null) {
                linearFade *= FadeY.Value(level.Camera.Y + level.Camera.Viewport.Height / 2f);
            }

            for (int i = 0; i < particles.Length; i++) {
                particles[i].Position -= dir * particles[i].Speed * Engine.DeltaTime;
                particles[i].Position += perp * MathF.Sin(particles[i].Sin) * particles[i].Speed * sineAmplitudeMult * Engine.DeltaTime;
                particles[i].Sin += Engine.DeltaTime;
            }
        }
    }
}