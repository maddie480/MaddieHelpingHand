using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    class CustomPlanets : Backdrop {
        private struct Planet {
            public MTexture[] Textures;
            public Vector2 Position;
        }

        private readonly Planet[] planets;
        private readonly float animationDelay;

        private const int mapWidth = 640;
        private const int mapHeight = 360;

        public CustomPlanets(int count, string directory, float animationDelay) {
            // look up all the planets in the folder, and group frames belonging to the same planets.
            List<MTexture> folderContents = GFX.Game.Textures.Keys.Where(path => path.StartsWith(directory + "/")).Select(path => GFX.Game[path]).ToList();
            Dictionary<string, List<MTexture>> planetList = new Dictionary<string, List<MTexture>>();
            foreach (MTexture texture in folderContents) {
                string planetName = Regex.Replace(texture.AtlasPath, "\\d+$", string.Empty);
                if (!planetList.TryGetValue(planetName, out List<MTexture> planetTextures)) {
                    planetTextures = new List<MTexture>();
                    planetList[planetName] = planetTextures;
                }
                planetTextures.Add(texture);
            }

            // now, pick planets from the pool and place them.
            planets = new Planet[count];
            for (int i = 0; i < planets.Length; i++) {
                planets[i].Textures = planetList[Calc.Random.Choose(planetList.Keys.ToList())].ToArray();
                planets[i].Position = new Vector2 {
                    X = Calc.Random.NextFloat(mapWidth),
                    Y = Calc.Random.NextFloat(mapHeight)
                };
            }

            this.animationDelay = animationDelay;
        }

        public override void Update(Scene scene) {
            base.Update(scene);
            Position += Speed * Engine.DeltaTime;
        }

        public override void Render(Scene scene) {
            Vector2 cameraPosition = (scene as Level).Camera.Position;
            Color color = Color * FadeAlphaMultiplier;
            for (int i = 0; i < planets.Length; i++) {
                Vector2 planetPosition = default;
                planetPosition.X = -32f + mod(planets[i].Position.X - cameraPosition.X * Scroll.X + Position.X, mapWidth);
                planetPosition.Y = -32f + mod(planets[i].Position.Y - cameraPosition.Y * Scroll.Y + Position.Y, mapHeight);
                planets[i].Textures[(int) ((Engine.Scene.TimeActive / animationDelay) % planets[i].Textures.Length)].DrawCentered(planetPosition, color);
            }
        }

        private float mod(float x, float m) {
            return (x % m + m) % m;
        }
    }
}
