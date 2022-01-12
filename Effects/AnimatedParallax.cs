using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
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
                    return orig;
                });
            }
        }


        private readonly List<MTexture> frames;
        private readonly float fps;

        private int currentFrame;
        private float currentFrameTimer;

        public AnimatedParallax(MTexture texture) : base(texture) {
            // remove the frame number, much like decals do.
            string texturePath = Regex.Replace(texture.AtlasPath, "\\d+$", string.Empty);

            // then load all frames from that prefix.
            frames = GFX.Game.GetAtlasSubtextures(texturePath);

            Match fpsCount = Regex.Match(texturePath, "[^0-9]((?:[0-9]+\\.)?[0-9]+)fps$");
            if (fpsCount.Success) {
                // we found an FPS count! use it.
                fps = float.Parse(fpsCount.Groups[1].Value);
            } else {
                // use 12 FPS by default, like decals.
                fps = 12f;
            }

            Texture = frames[0];
            currentFrame = 0;
            currentFrameTimer = 1f / fps;
        }

        public override void Update(Scene scene) {
            base.Update(scene);

            currentFrameTimer -= Engine.DeltaTime;
            if (currentFrameTimer < 0f) {
                currentFrameTimer += (1f / fps);
                currentFrame++;
                currentFrame %= frames.Count;
                Texture = frames[currentFrame];
            }
        }
    }
}
