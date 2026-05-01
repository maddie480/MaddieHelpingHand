using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public partial class CustomStarfield : Starfield {
        [GeneratedRegex("^([0-9]+)_[0-9]+$")]
        private static partial Regex capturePrefixes();

        private float fps;
        private MTexture[][] starAnimations;
        private float[] frameOffsets;

        public CustomStarfield(string[] paths, Color[] colors, float[] alphas, bool shuffle = true, float speed = 1f, float fps = 0)
            : base(colors[0], speed) {

            this.fps = fps;
            starAnimations = new MTexture[Stars.Length][];
            frameOffsets = new float[Stars.Length];

            MTexture[][][] textures = new MTexture[paths.Length][][];
            for (int i = 0; i < paths.Length; i++) {
                if (fps <= 0) {
                    // single frame for each star: [path][index] => GetAtlasSubtextures captures the index
                    List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("particles/" + paths[i] + "/");
                    textures[i] = new MTexture[atlasSubtextures.Count][];
                    for (int j = 0; j < atlasSubtextures.Count; j++) {
                        textures[i][j] = new MTexture[] { atlasSubtextures[j] };
                    }
                } else {
                    // animations for each star: [path][index]_[framenumber] => GetAtlasSubtextures captures the framenumber
                    string basePath = "particles/" + paths[i] + "/";
                    List<string> prefixes = GFX.Game.Textures.Keys
                        .Where(path => path.StartsWith(basePath))
                        .Select(path => {
                            // [path][index]_[framenumber] => [index]_[framenumber] => the regex captures [index]
                            Match capture = capturePrefixes().Match(path.Substring(basePath.Length));
                            if (!capture.Success) return null; // filtered out
                            return basePath + capture.Groups[1] + "_";
                        })
                        .Where(path => path != null)
                        .Order().Distinct()
                        .ToList();

                    textures[i] = new MTexture[prefixes.Count][];
                    for (int j = 0; j < prefixes.Count; j++) {
                        Logger.Verbose("MaxHelpingHand/CustomStarfield", $"Found subtexture prefix with path {paths[i]}: {prefixes[j]}");
                        textures[i][j] = GFX.Game.GetAtlasSubtextures(prefixes[j]).ToArray();
                    }
                }
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

                MTexture[][] atlasSubtextures = textures[textureIndex];
                Color blended = colors[colorIndex] * alphas[alphaIndex];

                // Distance, Color and texture index are computed from the same random number.
                // We are reversing the operation (Distance = 4 + randomNumber * 20) to find out the random number and use it to set the color and texture again.
                float randomNumber = (Stars[i].Distance - 4) / 20f;

                Stars[i].Color = Color.Lerp(blended, Color.Transparent, randomNumber * 0.5f);
                int texIndex = (int) Calc.Clamp(Ease.CubeIn(1f - randomNumber) * atlasSubtextures.Length, 0f, atlasSubtextures.Length - 1);
                Stars[i].Texture = atlasSubtextures[texIndex][0];
                starAnimations[i] = atlasSubtextures[texIndex];
                if (starAnimations[i].Length > 1) {
                    frameOffsets[i] = Calc.Random.NextSingle() * starAnimations[i].Length;
                }

                if (!shuffle) {
                    textureIndex = (textureIndex + 1) % textures.Length;
                    colorIndex = (colorIndex + 1) % colors.Length;
                    alphaIndex = (alphaIndex + 1) % alphas.Length;
                }
            }
        }

        public override void Update(Scene scene) {
            base.Update(scene);
            if (fps <= 0) return;

            for (int i = 0; i < Stars.Length; i++) {
                int currentFrame = ((int) (scene.TimeActive * fps + frameOffsets[i])) % starAnimations[i].Length;
                Stars[i].Texture = starAnimations[i][currentFrame];
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
