using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class SnowCustomColors : Snow {
        private static readonly Type particleType = typeof(Snow).GetNestedType("Particle", BindingFlags.NonPublic);
        private static readonly MethodInfo particleInit = particleType.GetMethod("Init");

        public SnowCustomColors(Color[] colors, float speedMin, float speedMax, int particleCount) : base(false) {
            var selfData = new DynData<Snow>(this);

            // redo the same operations as the vanilla constructor, but with our custom set of colors.
            selfData["colors"] = colors;
            selfData["blendedColors"] = new Color[colors.Length];

            // recreate the particles array with the correct length
            var particles = Array.CreateInstance(particleType, particleCount);
            selfData["particles"] = particles;

            for (int i = 0; i < particles.Length; i++) {
                // Particle is a private struct, so getting it gets a copy that we should set back afterwards.
                var particle = particles.GetValue(i);
                particleInit.Invoke(particle, [colors.Length, speedMin, speedMax]);
                particles.SetValue(particle, i);
            }
        }
    }
}