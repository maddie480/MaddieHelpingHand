using System;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.MaxHelpingHand.Module;
using MonoMod;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class SnowCustomColors : Snow {
        private Vector2 dir, perp;
        private float sineAmplitudeMult;
        private MTexture particleTexture;

        public SnowCustomColors(Color[] colors, float speedMin, float speedMax, int particleCount, float angle, float sineAmplitudeMult, string texturePath) : base(false) {
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
            // possible future ideas: support for multiple textures and/or animations, texture rotation/scale options?
            particleTexture = string.IsNullOrWhiteSpace(texturePath) ? Draw.Pixel : GFX.Game[texturePath];
        }

        [MonoModLinkTo("Celeste.Backdrop", "System.Void Update(Monocle.Scene)")]
        private void base_Update(Scene scene) {
            throw new NotImplementedException("WTF? MonoModLinkTo is supposed to have relinked calls to this method!");
        }

        public override void Update(Scene scene) {
            base_Update(scene);

            Level level = scene as Level;

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

        public override void Render(Scene scene) {
            if (Alpha <= 0f || visibleFade <= 0f || linearFade <= 0f) {
                return;
            }

            for (int i = 0; i < blendedColors.Length; i++) {
                blendedColors[i] = colors[i] * (Alpha * visibleFade * linearFade);
            }

            Camera camera = (scene as Level).Camera;
            Vector2 loopPadding = new Vector2(particleTexture.Width / 2f - 0.5f, particleTexture.Height / 2f - 0.5f);
            for (int i = 0; i < particles.Length; i++) {
                Color color = blendedColors[particles[i].Color];

                Vector2 position = new Vector2(
                    x: mod(particles[i].Position.X - camera.X * Scroll.X, 320f + loopPadding.X * 2f) - loopPadding.X,
                    y: mod(particles[i].Position.Y - camera.Y * Scroll.Y, 180f + loopPadding.Y * 2f) - loopPadding.Y);

                if (!MaxHelpingHandModule.ZoomOutEnabled) {
                    particleTexture.DrawCentered(position, color);
                } else {
                    for (float x = position.X; x < camera.Viewport.Width + loopPadding.X; x += 320f + loopPadding.X * 2f) {
                        for (float y = position.Y; y < camera.Viewport.Height + loopPadding.Y; y += 180f + loopPadding.Y * 2f) {
                            particleTexture.DrawCentered(new Vector2(x, y), color);
                        }
                    }
                }
            }
        }
    }
}