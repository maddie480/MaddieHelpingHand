using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class AnimatedParallax : Parallax {
        public static void Load() {
            IL.Celeste.MapData.ParseBackdrop += onParseBackdrop;
        }

        public static void Unload() {
            IL.Celeste.MapData.ParseBackdrop -= onParseBackdrop;
        }

        private static void onParseBackdrop(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchNewobj(typeof(Parallax)))) {
                Logger.Log("MaxHelpingHand/AnimatedParallax", $"Handling animated parallaxes at {cursor.Index} in IL for MapData.ParseBackdrop");

                cursor.EmitDelegate<Func<Parallax, Parallax>>(orig => {
                    if (orig.Texture?.AtlasPath?.StartsWith("bgs/MaxHelpingHand/animatedParallax/") ?? false) {
                        // nah, this is an ANIMATED parallax, mind you!
                        return new AnimatedParallax(orig.Texture);
                    }
                    if (orig.Texture?.AtlasPath?.StartsWith("bgs/MaxHelpingHand/hdParallax/") ?? false) {
                        // and this is an HD parallax.
                        return new HdParallax(orig.Texture);
                    }
                    if (orig.Texture?.AtlasPath?.StartsWith("bgs/MaxHelpingHand/animatedHdParallax/") ?? false) {
                        // ... why not both?
                        return new AnimatedHdParallax(orig.Texture);
                    }
                    return orig;
                });
            }
        }

        private class ParallaxMeta {
            public float? FPS { get; set; } = null;
            public string Frames { get; set; } = null;
        }


        private readonly List<MTexture> frames;
        private readonly int[] frameOrder;
        private readonly float fps;

        private int currentFrame;
        private float currentFrameTimer;

        public AnimatedParallax(MTexture texture) : base(texture) {
            // remove the frame number, much like decals do.
            string texturePath = Regex.Replace(texture.AtlasPath, "\\d+$", string.Empty);

            // then load all frames from that prefix.
            frames = GFX.Game.GetAtlasSubtextures(texturePath);

            // by default, the frames are just in order and last the same duration.
            frameOrder = new int[frames.Count];
            for (int i = 0; i < frameOrder.Length; i++) {
                frameOrder[i] = i;
            }

            Match fpsCount = Regex.Match(texturePath, "[^0-9]((?:[0-9]+\\.)?[0-9]+)fps$");
            if (fpsCount.Success) {
                // we found an FPS count! use it.
                fps = float.Parse(fpsCount.Groups[1].Value);
            } else {
                // use 12 FPS by default, like decals.
                fps = 12f;
            }

            if (Everest.Content.Map.TryGetValue("Graphics/Atlases/Gameplay/" + texturePath + ".meta", out ModAsset metaYaml) && metaYaml.Type == typeof(AssetTypeYaml)) {
                // the styleground has a metadata file! we should read it.
                ParallaxMeta meta;
                using (TextReader r = new StreamReader(metaYaml.Stream)) {
                    meta = YamlHelper.Deserializer.Deserialize<ParallaxMeta>(r);
                }

                if (meta.FPS != null) {
                    fps = meta.FPS.Value;
                }

                if (meta.Frames != null) {
                    frameOrder = Calc.ReadCSVIntWithTricks(meta.Frames);
                }
            }

            Texture = frames[frameOrder[0]];
            currentFrame = 0;
            currentFrameTimer = 1f / fps;
        }

        public override void Update(Scene scene) {
            base.Update(scene);

            if (IsVisible(scene as Level)) {
                currentFrameTimer -= Engine.DeltaTime;
                if (currentFrameTimer < 0f) {
                    currentFrameTimer += (1f / fps);
                    currentFrame++;
                    currentFrame %= frameOrder.Length;
                    Texture = frames[frameOrder[currentFrame]];
                }
            }
        }
    }
}
