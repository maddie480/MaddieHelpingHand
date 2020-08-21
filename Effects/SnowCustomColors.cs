using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class SnowCustomColors : Snow {
        private static MethodInfo particleInit = typeof(Snow).GetNestedType("Particle", BindingFlags.NonPublic).GetMethod("Init");

        public SnowCustomColors(Color[] colors, bool foreground) : base(foreground) {
            DynData<Snow> selfData = new DynData<Snow>(this);

            // redo the same operations as the vanilla constructor, but with our custom set of colors.
            selfData["colors"] = colors;
            selfData["blendedColors"] = new Color[colors.Length];
            Array particles = selfData.Get<Array>("particles");
            int speedMin = foreground ? 120 : 40;
            int speedMax = foreground ? 300 : 100;
            for (int i = 0; i < particles.Length; i++) {
                // Particle is a private struct, so getting it gets a copy that we should set back afterwards.
                object particle = particles.GetValue(i);
                particleInit.Invoke(particle, new object[] { colors.Length, speedMin, speedMax });
                particles.SetValue(particle, i);
            }
        }
    }
}