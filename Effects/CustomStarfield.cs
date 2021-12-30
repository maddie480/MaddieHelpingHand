using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Effects
{
    public class CustomStarfield : Starfield
    {
        public CustomStarfield(string[] paths, Color[] colors, float[] alphas, bool shuffle = true, float speed = 1f) : base(colors[0], speed)
        {
            List<MTexture>[] textures = new List<MTexture>[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                textures[i] = GFX.Game.GetAtlasSubtextures("particles/" + paths[i] + "/");
            }

            int texture_i = 0;
            int color_i = 0;
            int alpha_i = 0;

            for (int i = 0; i < Stars.Length; i++)
            {
                if (shuffle)
                {
                    texture_i = Calc.Random.Range(0, textures.Length - 1);
                    color_i = Calc.Random.Range(0, colors.Length - 1);
                    alpha_i = Calc.Random.Range(0, alphas.Length - 1);
                }

                List<MTexture> atlasSubtextures = textures[texture_i];
                Color blended = colors[color_i] * alphas[alpha_i];

                float important_number = Stars[i].Distance / 20f - 4f;
                Stars[i].Color = Color.Lerp(blended, Color.Transparent, important_number * 0.5f);
                int tex_index = (int)Calc.Clamp(Ease.CubeIn(1f - important_number) * (float)atlasSubtextures.Count, 0f, atlasSubtextures.Count - 1);
                Stars[i].Texture = atlasSubtextures[tex_index];

                if (!shuffle)
                {
                    texture_i++;
                    color_i++;
                    alpha_i++;

                    if (texture_i >= textures.Length || color_i >= colors.Length || alpha_i >= alphas.Length)
                    {
                        texture_i = 0;
                        color_i = 0;
                        alpha_i = 0;
                    }
                }
            }
        }
    }
}