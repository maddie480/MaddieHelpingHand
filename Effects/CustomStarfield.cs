using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class CustomStarfield : Starfield {
        public CustomStarfield(string[] paths, Color[] colors, float[] alphas, bool shuffle = true, float speed = 1f) : base(colors[0], speed) {
            List<MTexture>[] textures = new List<MTexture>[paths.Length];
            for (int i = 0; i < paths.Length; i++) {
                textures[i] = GFX.Game.GetAtlasSubtextures("particles/" + paths[i] + "/");
            }

            int textureIndex = 0;
            int colorIndex = 0;
            int alphaIndex = 0;

            for (int i = 0; i < Stars.Length; i++) {
                if (shuffle) {
                    textureIndex = Calc.Random.Range(0, textures.Length);
                    colorIndex = Calc.Random.Range(0, colors.Length);
                    alphaIndex = Calc.Random.Range(0, alphas.Length);
                }

                List<MTexture> atlasSubtextures = textures[textureIndex];
                Color blended = colors[colorIndex] * alphas[alphaIndex];

                // Distance, Color and texture index are computed from the same random number.
                // We are reversing the operation (Distance = 4 + randomNumber * 20) to find out the random number and use it to set the color and texture again.
                float randomNumber = (Stars[i].Distance - 4) / 20f;

                Stars[i].Color = Color.Lerp(blended, Color.Transparent, randomNumber * 0.5f);
                int texIndex = (int) Calc.Clamp(Ease.CubeIn(1f - randomNumber) * atlasSubtextures.Count, 0f, atlasSubtextures.Count - 1);
                Stars[i].Texture = atlasSubtextures[texIndex];

                if (!shuffle) {
                    textureIndex = (textureIndex + 1) % textures.Length;
                    colorIndex = (colorIndex + 1) % colors.Length;
                    alphaIndex = (alphaIndex + 1) % alphas.Length;
                }
            }
        }

        public override void Render(Scene scene) {
            float origFadeAlpha = FadeAlphaMultiplier;
            FadeAlphaMultiplier *= CustomBackdrop.GetFadeAlphaFor(this, scene);

            base.Render(scene);

            FadeAlphaMultiplier = origFadeAlpha;
        }
    }
}