using FlaglinesAndSuch;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class SineAnimatedParallax : SineParallaxStyleground {
        private static ILHook hookOnStylegroundConstructor;
        private static Hook hookOnStylegroundUpdate;

        // if we do not do this ourselves, the compiler is going to do it for us... using a nested class,
        // which makes the game crash on startup due to this class extending a type that does not exist if you don't have Flaglines and Such installed.
        private static ILContext.Manipulator _onParseBackdrop = onParseBackdrop;
        private static Func<Instruction, bool> _isSineParallaxStylegroundGettingConstructed = isSineParallaxStylegroundGettingConstructed;
        private static Func<BinaryPacker.Element, bool> _isAnimatedParallax = isAnimatedParallax;

        public static void Load() {
            hookOnStylegroundConstructor = new ILHook(typeof(Class1).GetMethod("Level_OnLoadBackdrop", BindingFlags.NonPublic | BindingFlags.Instance), _onParseBackdrop);
            hookOnStylegroundUpdate = new Hook(typeof(SineParallaxStyleground).GetMethod("Update"), typeof(SineAnimatedParallax).GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Static));
        }

        public static void Unload() {
            hookOnStylegroundConstructor?.Dispose();
            hookOnStylegroundConstructor = null;

            hookOnStylegroundUpdate?.Dispose();
            hookOnStylegroundUpdate = null;
        }

        private static void onParseBackdrop(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(_isSineParallaxStylegroundGettingConstructed)) {
                Logger.Log("MaxHelpingHand/SineAnimatedParallax", $"Handling animated parallaxes at {cursor.Index} in IL for FlaglinesAndSuch's ParseBackdrop");

                // this results in the following nonsensical code:
                // bool isAnimated = child.Attr("Texture")?.StartsWith("bgs/MaxHelpingHand/animatedParallax/") ?? false;
                // (isAnimated ? new SineAnimatedParallax : new SineParallaxStyleground)(parameters)
                cursor.Emit(OpCodes.Ldarg_2);
                cursor.EmitDelegate<Func<BinaryPacker.Element, bool>>(_isAnimatedParallax);
                cursor.Emit(OpCodes.Brfalse, cursor.Next);
                cursor.Emit(OpCodes.Newobj, typeof(SineAnimatedParallax).GetConstructor(new Type[] { typeof(string), typeof(float), typeof(float), typeof(float), typeof(float),
                    typeof(float), typeof(float), typeof(bool), typeof(bool), typeof(float), typeof(float), typeof(float), typeof(bool) }));
                cursor.Emit(OpCodes.Br, cursor.Next.Next);
            }
        }

        // having inline methods makes the compiler generate an inner class, which crashes if Flaglines And Such is not installed because Everest's Lua loader tries to get info on it.
        // so we need to have those be separate.
        private static bool isSineParallaxStylegroundGettingConstructed(Instruction instr) {
            return instr.MatchNewobj(typeof(SineParallaxStyleground));
        }
        private static bool isAnimatedParallax(BinaryPacker.Element child) {
            return child.Attr("Texture")?.StartsWith("bgs/MaxHelpingHand/animatedParallax/") ?? false;
        }


        private readonly List<MTexture> frames;
        private readonly float fps;

        private int currentFrame;
        private float currentFrameTimer;

        public SineAnimatedParallax(string textureString, float posx, float posy, float speedx, float speedy, float scrollx, float scrolly, bool loopx, bool loopy, float A, float f, float x, bool vert)
            : base(textureString, posx, posy, speedx, speedy, scrollx, scrolly, loopx, loopy, A, f, x, vert) {

            // remove the frame number, much like decals do.
            string texturePath = Regex.Replace(Texture.AtlasPath, "\\d+$", string.Empty);

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

        // it seems just overriding Update like everyone would, ends up with base.Update turning into this.Update and causing a stack overflow crash.
        // ... oh well.
        private static void Update(Action<SineParallaxStyleground, Scene> orig, SineParallaxStyleground self, Scene scene) {
            orig(self, scene);

            if (self is SineAnimatedParallax animated) {
                animated.currentFrameTimer -= Engine.DeltaTime;
                if (animated.currentFrameTimer < 0f) {
                    animated.currentFrameTimer += (1f / animated.fps);
                    animated.currentFrame++;
                    animated.currentFrame %= animated.frames.Count;
                    animated.Texture = animated.frames[animated.currentFrame];
                }
            }
        }
    }
}
