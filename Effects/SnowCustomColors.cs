using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class SnowCustomColors : Snow {
        public SnowCustomColors(Color[] colors, float speedMin, float speedMax, int particleCount) : base(false) {
            // redo the same operations as the vanilla constructor, but with our custom set of colors.
            this.colors = colors;
            blendedColors = new Color[colors.Length];

            // recreate the particles array with the correct length
            particles = new Particle[particleCount];

            for (int i = 0; i < particles.Length; i++) {
                // Particle is a private struct, so getting it gets a copy that we should set back afterwards.
                var particle = particles[i];
                particle.Init(colors.Length, speedMin, speedMax);
                particles.SetValue(particle, i);
            }
        }
    }
}