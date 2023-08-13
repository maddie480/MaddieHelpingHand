using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.IO;
using System.Reflection;
using static Celeste.DustStyles;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/GradientDustTrigger")]
    public class GradientDustTrigger : Trigger {
        // a white dust style.
        private static DustStyle white = new DustStyle {
            EdgeColors = new Vector3[] { Color.White.ToVector3(), Color.White.ToVector3(), Color.White.ToVector3() },
            EyeColor = Color.White,
            EyeTextures = "danger/dustcreature/eyes"
        };

        // a BlendState forked from AlphaBlend that takes the minimum color between the existing image and what is drawn over it.
        private static BlendState substractive = new BlendState() {
            ColorSourceBlend = Blend.One,
            AlphaSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Min,
            AlphaBlendFunction = BlendFunction.Min
        };

        static GradientDustTrigger() {
            if (File.Exists("BuildIsXNA.txt")) {
                // I... don't get it. why **max** to get it to subtract? anyway, this works.
                substractive.ColorBlendFunction = BlendFunction.Max;
                substractive.AlphaBlendFunction = BlendFunction.Max;
            }
        }

        public static void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
            Everest.Events.Level.OnExit += onLevelExit;

            On.Celeste.DustStyles.Get_Scene += onGetDustStyle;
            On.Celeste.DustEdges.BeforeRender += onDustEdgesBeforeRender;

            eyeballHook = new ILHook(typeof(DustGraphic).GetNestedType("Eyeballs", BindingFlags.NonPublic).GetMethod("Render"), modDustEyesRender);
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
            Everest.Events.Level.OnExit -= onLevelExit;

            On.Celeste.DustStyles.Get_Scene -= onGetDustStyle;
            On.Celeste.DustEdges.BeforeRender -= onDustEdgesBeforeRender;

            eyeballHook?.Dispose();
            eyeballHook = null;
        }

        private static void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (MaxHelpingHandModule.Instance.Session.GradientDustImagePath != null && !hookEnabled) {
                applyGradient(MaxHelpingHandModule.Instance.Session.GradientDustImagePath, MaxHelpingHandModule.Instance.Session.GradientDustScrollSpeed);
            }
        }

        private static void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow) {
            discardGradient();
        }

        private static bool hookEnabled = false;

        private static Color[] colors;
        private static MTexture image;
        private static float speed;

        private static ILHook eyeballHook;

        // ==== start non-static stuff
        private string triggerImage;
        private float triggerSpeed;

        public GradientDustTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            triggerImage = data.Attr("gradientImage", "MaxHelpingHand/gradientdustbunnies/bluegreen");
            triggerSpeed = data.Float("scrollSpeed", 50f);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (string.IsNullOrEmpty(triggerImage)) {
                discardGradient();
                MaxHelpingHandModule.Instance.Session.GradientDustImagePath = null;
            } else {
                applyGradient(triggerImage, triggerSpeed);
                MaxHelpingHandModule.Instance.Session.GradientDustImagePath = triggerImage;
                MaxHelpingHandModule.Instance.Session.GradientDustScrollSpeed = triggerSpeed;
            }
        }
        // ==== end non-static stuff

        private static void applyGradient(string imagePath, float scrollSpeed) {
            Logger.Log(LogLevel.Debug, "MaxHelpingHand/GradientDustTrigger", "Applying gradient dust with path: " + imagePath + " and scroll speed " + scrollSpeed);

            image = GFX.Game[imagePath];
            speed = scrollSpeed;

            // get the image colors as a Color[], this will be useful to color the dust bunny eyeballs.
            Color[] colorsFullArray = new Color[image.Width * image.Height];
            image.Texture.Texture.GetData(colorsFullArray);
            colors = new Color[image.Width];
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = colorsFullArray[i];
            }

            hookEnabled = true;
        }

        private static void discardGradient() {
            Logger.Log(LogLevel.Debug, "MaxHelpingHand/GradientDustTrigger", "Discarding gradient dust");

            hookEnabled = false;

            colors = null;
            image = null;
        }

        private static DustStyle onGetDustStyle(On.Celeste.DustStyles.orig_Get_Scene orig, Scene scene) {
            if (!hookEnabled) return orig(scene);

            // make dust white: we're applying a color filter on top of it.
            return white;
        }

        private static void onDustEdgesBeforeRender(On.Celeste.DustEdges.orig_BeforeRender orig, DustEdges self) {
            orig(self);
            if (!hookEnabled) return;

            // draw our image in ""substractive"" mode over the resort dust layer.
            float shift = (Engine.Scene.TimeActive * speed) % image.Width;
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, substractive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            for (float offset = -shift; offset < 320; offset += image.Width) {
                Draw.SpriteBatch.Draw(image.Texture.Texture, new Vector2(offset, 0), Color.White);
            }
            Draw.SpriteBatch.End();
        }

        private static void modDustEyesRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(instr => instr.MatchCallvirt<MTexture>("DrawCentered"))) {
                Logger.Log("MaxHelpingHand/GradientDustController", $"Proxying DrawCentered calls at {cursor.Index} in IL for Eyeballs.Render");

                cursor.Remove();
                cursor.EmitDelegate<Action<MTexture, Vector2, Color, float>>((texture, position, color, scale) => {
                    if (!hookEnabled) {
                        texture.DrawCentered(position, color, scale);
                        return;
                    }

                    // compute the eye color to make the color match the dust edges.
                    float shift = (Engine.Scene.TimeActive * speed) % image.Width;
                    float cameraX = (Engine.Scene as Level)?.Camera?.Left ?? 0f;
                    texture.DrawCentered(position, colors[mod((int) (position.X - cameraX + shift), image.Width)], scale);
                });
            }
        }

        private static int mod(int a, int b) {
            float mod = a % b;
            while (mod < 0) {
                mod += b;
            }
            return (int) mod;
        }
    }
}
