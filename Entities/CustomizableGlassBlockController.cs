using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// A non-global and customizable version of the vanilla GlassBlockBg class.
    /// This draws a background for all CustomizableGlassBlocks in the room.
    /// </summary>
    [CustomEntity("MaxHelpingHand/CustomizableGlassBlockController")]
    [Tracked]
    public class CustomizableGlassBlockController : Entity {
        public static void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
        }

        private static void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            if (MaxHelpingHandModule.Instance.Session.GlassBlockCurrentSettings != null
                && self.Session.LevelData != null // happens if we are loading a save in a room that got deleted
                && !self.Session.LevelData.Entities.Any(entity => entity.Name == "MaxHelpingHand/CustomizableGlassBlockController")) {

                // we have customizable glass block settings, and are entering a room with no controller: spawn one.
                EntityData restoredData = new EntityData();
                restoredData.Values = new Dictionary<string, object>() {
                    { "starColors", MaxHelpingHandModule.Instance.Session.GlassBlockCurrentSettings.StarColors },
                    { "bgColor", MaxHelpingHandModule.Instance.Session.GlassBlockCurrentSettings.BgColor },
                    { "wavy", MaxHelpingHandModule.Instance.Session.GlassBlockCurrentSettings.Wavy },
                    { "persistent", true }
                };

                self.Add(new CustomizableGlassBlockController(restoredData, Vector2.Zero));
            }

            orig(self, playerIntro, isFromLoader);
        }

        private struct Star {
            public Vector2 Position;
            public MTexture Texture;
            public Color Color;
            public Vector2 Scroll;
        }

        private struct Ray {
            public Vector2 Position;
            public float Width;
            public float Length;
            public Color Color;
        }

        private const int StarCount = 100;
        private const int RayCount = 50;

        private Star[] stars = new Star[StarCount];
        private Ray[] rays = new Ray[RayCount];

        private VertexPositionColor[] verts = new VertexPositionColor[2700];

        private Vector2 rayNormal = new Vector2(-5f, -8f).SafeNormalize();

        private VirtualRenderTarget beamsTarget;
        private VirtualRenderTarget starsTarget;

        private bool hasBlocks;

        private Color[] starColors;
        private Color bgColor;

        private float alpha = 1f;

        public CustomizableGlassBlockController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            // parse the "starColors" and "bgColor" parameters
            string[] starColorsAsStrings = data.Attr("starColors").Split(',');
            starColors = new Color[starColorsAsStrings.Length];
            for (int i = 0; i < starColors.Length; i++) {
                starColors[i] = Calc.HexToColor(starColorsAsStrings[i]);
            }
            bgColor = Calc.HexToColor(data.Attr("bgColor"));

            // do some more initialization.
            Add(new BeforeRenderHook(BeforeRender));
            Depth = -9990;

            if (data.Bool("wavy", false)) {
                Add(new DisplacementRenderHook(OnDisplacementRender));
            }

            // update session
            if (data.Bool("persistent")) {
                MaxHelpingHandModule.Instance.Session.GlassBlockCurrentSettings = new MaxHelpingHandSession.CustomizableGlassBlockState() {
                    StarColors = data.Attr("starColors"),
                    BgColor = data.Attr("bgColor"),
                    Wavy = data.Bool("wavy", false)
                };
            } else {
                MaxHelpingHandModule.Instance.Session.GlassBlockCurrentSettings = null;
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // check if there is already a CustomizableGlassBlockController in the room (supposedly from the previous room)
            // so that we can carry values over.
            if (scene.Tracker.GetEntities<CustomizableGlassBlockController>()
                .Find(controller => controller != this && !(controller is CustomizableGlassBlockAreaController)) is CustomizableGlassBlockController controllerFromPreviousRoom) {

                // carry over the state so that stars/rays don't change places.
                stars = (Star[]) controllerFromPreviousRoom.stars.Clone();
                rays = controllerFromPreviousRoom.rays;

                if (!controllerFromPreviousRoom.starColors.SequenceEqual(starColors)) {
                    // start out transparent and fade in since starColors changed.
                    alpha = 0f;

                    // reroll colors since they changed. we copy the Stars so that the original ones are unchanged.
                    for (int i = 0; i < stars.Length; i++) {
                        stars[i] = new Star {
                            Position = stars[i].Position,
                            Texture = stars[i].Texture,
                            Color = Calc.Random.Choose(starColors),
                            Scroll = stars[i].Scroll
                        };
                    }
                } else if (controllerFromPreviousRoom.bgColor != bgColor) {
                    // start out transparent and fade in since bgColor changed.
                    alpha = 0f;
                }
            } else {
                // initialize stars and rays from scratch like vanilla does.
                List<MTexture> starTextures = GFX.Game.GetAtlasSubtextures("particles/stars/");
                for (int i = 0; i < stars.Length; i++) {
                    stars[i].Position.X = Calc.Random.Next(MaxHelpingHandModule.GameplayWidth);
                    stars[i].Position.Y = Calc.Random.Next(MaxHelpingHandModule.GameplayHeight);
                    stars[i].Texture = Calc.Random.Choose(starTextures);
                    stars[i].Color = Calc.Random.Choose(starColors);
                    stars[i].Scroll = Vector2.One * Calc.Random.NextFloat(0.05f);
                }

                for (int j = 0; j < rays.Length; j++) {
                    rays[j].Position.X = Calc.Random.Next(MaxHelpingHandModule.GameplayWidth);
                    rays[j].Position.Y = Calc.Random.Next(MaxHelpingHandModule.GameplayHeight);
                    rays[j].Width = Calc.Random.Range(4f, 16f);
                    rays[j].Length = Calc.Random.Choose(48, 96, 128);
                    rays[j].Color = Color.White * Calc.Random.Range(0.2f, 0.4f);
                }
            }
        }

        private void ensureBufferIsCorrect() {
            if (starsTarget == null || starsTarget.Width != MaxHelpingHandModule.GameplayWidth || starsTarget.Height != MaxHelpingHandModule.GameplayHeight) {
                starsTarget?.Dispose();
                starsTarget = VirtualContent.CreateRenderTarget("customizable-glass-block-surfaces", MaxHelpingHandModule.GameplayWidth, MaxHelpingHandModule.GameplayHeight);
            }
        }

        private void BeforeRender() {
            List<CustomizableGlassBlock> glassBlocks = getGlassBlocksToAffect().ToList();
            hasBlocks = (glassBlocks.Count > 0);
            if (!hasBlocks) {
                return;
            }

            Camera camera = (Scene as Level).Camera;
            int screenWidth = MaxHelpingHandModule.GameplayWidth;
            int screenHeight = MaxHelpingHandModule.GameplayHeight;

            // draw stars
            ensureBufferIsCorrect();

            Engine.Graphics.GraphicsDevice.SetRenderTarget(starsTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            Vector2 origin = new Vector2(8f, 8f);
            for (int i = 0; i < stars.Length; i++) {
                MTexture starTexture = stars[i].Texture;
                Color starColor = stars[i].Color * alpha;
                Vector2 starScroll = stars[i].Scroll;
                Vector2 starActualPosition = default;
                starActualPosition.X = Mod(stars[i].Position.X - camera.X * (1f - starScroll.X), screenWidth);
                starActualPosition.Y = Mod(stars[i].Position.Y - camera.Y * (1f - starScroll.Y), screenHeight);
                starTexture.Draw(starActualPosition, origin, starColor);

                if (starActualPosition.X < origin.X) {
                    starTexture.Draw(starActualPosition + new Vector2(screenWidth, 0f), origin, starColor);
                } else if (starActualPosition.X > screenWidth - origin.X) {
                    starTexture.Draw(starActualPosition - new Vector2(screenWidth, 0f), origin, starColor);
                }

                if (starActualPosition.Y < origin.Y) {
                    starTexture.Draw(starActualPosition + new Vector2(0f, screenHeight), origin, starColor);
                } else if (starActualPosition.Y > screenHeight - origin.Y) {
                    starTexture.Draw(starActualPosition - new Vector2(0f, screenHeight), origin, starColor);
                }
            }
            Draw.SpriteBatch.End();

            // draw rays/beams
            int vertex = 0;
            for (int j = 0; j < rays.Length; j++) {
                Vector2 rayPosition = default;
                rayPosition.X = Mod(rays[j].Position.X - camera.X * 0.9f, screenWidth);
                rayPosition.Y = Mod(rays[j].Position.Y - camera.Y * 0.9f, screenHeight);
                DrawRay(rayPosition, ref vertex, ref rays[j]);
                if (rayPosition.X < 64f) {
                    DrawRay(rayPosition + new Vector2(screenWidth, 0f), ref vertex, ref rays[j]);
                } else if (rayPosition.X > (screenWidth - 64)) {
                    DrawRay(rayPosition - new Vector2(screenWidth, 0f), ref vertex, ref rays[j]);
                }
                if (rayPosition.Y < 64f) {
                    DrawRay(rayPosition + new Vector2(0f, screenHeight), ref vertex, ref rays[j]);
                } else if (rayPosition.Y > (screenHeight - 64)) {
                    DrawRay(rayPosition - new Vector2(0f, screenHeight), ref vertex, ref rays[j]);
                }
            }

            if (beamsTarget == null) {
                beamsTarget = VirtualContent.CreateRenderTarget("customizable-glass-block-beams", screenWidth, screenHeight);
            }

            Engine.Graphics.GraphicsDevice.SetRenderTarget(beamsTarget);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            GFX.DrawVertices(Matrix.Identity, verts, vertex);

            // if fading in, update the alpha value to fade in in ~0.25 seconds.
            if (alpha != 1f) {
                alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime * 4f);
            }
        }

        private void DrawRay(Vector2 position, ref int vertex, ref Ray ray) {
            Vector2 value = new Vector2(0f - rayNormal.Y, rayNormal.X);
            Vector2 value2 = rayNormal * ray.Width * 0.5f;
            Vector2 value3 = value * ray.Length * 0.25f * 0.5f;
            Vector2 value4 = value * ray.Length * 0.5f * 0.5f;
            Vector2 v = position + value2 - value3 - value4;
            Vector2 v2 = position - value2 - value3 - value4;
            Vector2 vector = position + value2 - value3;
            Vector2 vector2 = position - value2 - value3;
            Vector2 vector3 = position + value2 + value3;
            Vector2 vector4 = position - value2 + value3;
            Vector2 v3 = position + value2 + value3 + value4;
            Vector2 v4 = position - value2 + value3 + value4;
            Color transparent = Color.Transparent;
            Color color = ray.Color * alpha;
            Quad(ref vertex, v, vector, vector2, v2, transparent, color, color, transparent);
            Quad(ref vertex, vector, vector3, vector4, vector2, color, color, color, color);
            Quad(ref vertex, vector3, v3, v4, vector4, color, transparent, transparent, color);
        }

        private void Quad(ref int vertex, Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3, Color c0, Color c1, Color c2, Color c3) {
            verts[vertex].Position.X = v0.X;
            verts[vertex].Position.Y = v0.Y;
            verts[vertex++].Color = c0;
            verts[vertex].Position.X = v1.X;
            verts[vertex].Position.Y = v1.Y;
            verts[vertex++].Color = c1;
            verts[vertex].Position.X = v2.X;
            verts[vertex].Position.Y = v2.Y;
            verts[vertex++].Color = c2;
            verts[vertex].Position.X = v0.X;
            verts[vertex].Position.Y = v0.Y;
            verts[vertex++].Color = c0;
            verts[vertex].Position.X = v2.X;
            verts[vertex].Position.Y = v2.Y;
            verts[vertex++].Color = c2;
            verts[vertex].Position.X = v3.X;
            verts[vertex].Position.Y = v3.Y;
            verts[vertex++].Color = c3;
        }

        public override void Render() {
            if (hasBlocks) {
                Vector2 position = (Scene as Level).Camera.Position;
                IEnumerable<CustomizableGlassBlock> glassBlocks = getGlassBlocksToAffect();

                foreach (CustomizableGlassBlock glassBlock in glassBlocks) {
                    Draw.Rect(glassBlock.X + glassBlock.ShakeVector.X, glassBlock.Y + glassBlock.ShakeVector.Y,
                        glassBlock.Width, glassBlock.Height, bgColor * alpha * glassBlock.Alpha);
                }

                if (starsTarget != null && !starsTarget.IsDisposed) {
                    foreach (CustomizableGlassBlock glassBlock in glassBlocks) {
                        Rectangle target = new Rectangle((int) (glassBlock.X + glassBlock.ShakeVector.X - position.X), (int) (glassBlock.Y + glassBlock.ShakeVector.Y - position.Y),
                            (int) glassBlock.Width, (int) glassBlock.Height);
                        Draw.SpriteBatch.Draw(starsTarget, glassBlock.Position + glassBlock.ShakeVector, target, Color.White * glassBlock.Alpha);
                    }
                }

                if (beamsTarget != null && !beamsTarget.IsDisposed) {
                    foreach (CustomizableGlassBlock glassBlock in glassBlocks) {
                        Rectangle target = new Rectangle((int) (glassBlock.X + glassBlock.ShakeVector.X - position.X), (int) (glassBlock.Y + glassBlock.ShakeVector.Y - position.Y),
                            (int) glassBlock.Width, (int) glassBlock.Height);
                        Draw.SpriteBatch.Draw((RenderTarget2D) beamsTarget, glassBlock.Position + glassBlock.ShakeVector, target, Color.White * glassBlock.Alpha);
                    }
                }
            }
        }

        protected virtual IEnumerable<CustomizableGlassBlock> getGlassBlocksToAffect() {
            // all of them
            return Scene.Tracker.GetEntities<CustomizableGlassBlock>().OfType<CustomizableGlassBlock>();
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            Dispose();
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);
            Dispose();
        }

        public void Dispose() {
            if (starsTarget != null && !starsTarget.IsDisposed) {
                starsTarget.Dispose();
            }

            if (beamsTarget != null && !beamsTarget.IsDisposed) {
                beamsTarget.Dispose();
            }
            starsTarget = null;
            beamsTarget = null;
        }

        private float Mod(float x, float m) {
            return (x % m + m) % m;
        }

        private void OnDisplacementRender() {
            IEnumerable<CustomizableGlassBlock> blocks = getGlassBlocksToAffect();
            foreach (Entity block in blocks) {
                Draw.Rect(block.X + 1, block.Y + 1, block.Width - 2, block.Height - 2, new Color(0.5f, 0.5f, 0.2f, 1f));
            }
        }
    }
}
