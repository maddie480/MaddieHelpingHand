using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    class CustomStars : Backdrop {
		private struct Star {
			public Vector2 Position;
			public int TextureSet;
			public float Timer;
			public float Rate;
		}

		private int? starCount;
		private Color? tint;

		private Star[] stars;
		private Color[] colors;

		private List<List<MTexture>> textures;

		private float falling = 0f;
		private Vector2 center;

		public CustomStars(int? starCount, Color? tint, string spriteDirectory) {
			this.starCount = starCount;
			this.tint = tint;

			// look up all the stars in the folder, and group frames belonging to the same stars.
			textures = new List<List<MTexture>>();
			List<MTexture> folderContents = GFX.Game.Textures.Keys.Where(path => path.StartsWith(spriteDirectory + "/")).Select(path => GFX.Game[path]).ToList();
			Dictionary<string, List<MTexture>> starList = new Dictionary<string, List<MTexture>>();
			foreach (MTexture texture in folderContents) {
				string starName = Regex.Replace(texture.AtlasPath, "\\d+$", string.Empty);
				if (!starList.TryGetValue(starName, out List<MTexture> starTextures)) {
					starTextures = new List<MTexture>();
					starList[starName] = starTextures;
				}
				starTextures.Add(texture);
			}

			// be sure that the star sprites are ordered.
			foreach (List<MTexture> list in starList.Values) {
				list.Sort((a, b) => a.AtlasPath.CompareTo(b.AtlasPath));
			}

			// then add tlem to the texture list for the game to use them.
			textures.AddRange(starList.Values);

			center = new Vector2(textures[0][0].Width, textures[0][0].Height) / 2f;
			stars = new Star[starCount ?? 100];
			for (int i = 0; i < stars.Length; i++) {
				stars[i] = new Star {
					Position = new Vector2(Calc.Random.NextFloat(320f), Calc.Random.NextFloat(180f)),
					Timer = Calc.Random.NextFloat((float) Math.PI * 2f),
					Rate = 2f + Calc.Random.NextFloat(2f),
					TextureSet = Calc.Random.Next(textures.Count)
				};
			}
			colors = new Color[8];
			for (int i = 0; i < colors.Length; i++) {
				colors[i] = (tint ?? Color.Teal) * 0.7f * (1f - (float) i / colors.Length);
			}
		}

		public override void Update(Scene scene) {
			base.Update(scene);
			if (Visible) {
				Level level = scene as Level;
				for (int i = 0; i < stars.Length; i++) {
					stars[i].Timer += Engine.DeltaTime * stars[i].Rate;
				}
				if (level.Session.Dreaming) {
					falling += Engine.DeltaTime * 12f;
				}
			}
		}

		public override void Render(Scene scene) {
			Draw.Rect(0f, 0f, 320f, 180f, Color.Black);
			Level level = scene as Level;
			Color color = tint ?? (level.Session.Dreaming ? Color.Teal * 0.7f : Color.White);
			int count = starCount ?? (level.Session.Dreaming ? 100 : 50);
			for (int i = 0; i < count; i++) {
				List<MTexture> starTextures = textures[stars[i].TextureSet];
				int starFrame = (int) ((Math.Sin(stars[i].Timer) + 1.0) / 2.0 * starTextures.Count);
				starFrame %= starTextures.Count;
				Vector2 position = stars[i].Position;
				MTexture frameToRender = starTextures[starFrame];
				if (level.Session.Dreaming) {
					position.Y -= level.Camera.Y;
					position.Y += falling * stars[i].Rate;
					position.Y %= 180f;
					if (position.Y < 0f) {
						position.Y += 180f;
					}
					for (int j = 0; j < colors.Length; j++) {
						frameToRender.Draw(position - Vector2.UnitY * j, center, colors[j]);
					}
				}
				frameToRender.Draw(position, center, color);
			}
		}
	}
}
