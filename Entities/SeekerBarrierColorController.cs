using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/SeekerBarrierColorController")]
    public class SeekerBarrierColorController : Entity {
        public static void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
        }

        private static void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (MaxHelpingHandModule.Instance.Session.SeekerBarrierCurrentColors != null
                && self.Session.LevelData != null // happens if we are loading a save in a room that got deleted
                && !self.Session.LevelData.Entities.Any(entity =>
                    entity.Name == "MaxHelpingHand/SeekerBarrierColorController" || entity.Name == "MaxHelpingHand/SeekerBarrierColorControllerDisabler")) {

                // we have a barrier color, and are entering a room with no controller: spawn one.
                EntityData restoredData = new EntityData();
                restoredData.Values = new Dictionary<string, object>() {
                    { "color", MaxHelpingHandModule.Instance.Session.SeekerBarrierCurrentColors.Color },
                    { "particleColor", MaxHelpingHandModule.Instance.Session.SeekerBarrierCurrentColors.ParticleColor },
                    { "transparency", MaxHelpingHandModule.Instance.Session.SeekerBarrierCurrentColors.Transparency },
                    { "particleTransparency", MaxHelpingHandModule.Instance.Session.SeekerBarrierCurrentColors.ParticleTransparency },
                    { "particleDirection", MaxHelpingHandModule.Instance.Session.SeekerBarrierCurrentColors.ParticleDirection },
                    { "persistent", true }
                };

                self.Add(new SeekerBarrierColorController(restoredData, Vector2.Zero));
                self.Entities.UpdateLists();
            }
        }


        private static bool seekerBarrierRendererHooked = false;

        // the seeker controller on the current screen.
        private static SeekerBarrierColorController controllerOnScreen;

        // during transitions: the seeker controller on the next screen, and the progress between both screens.
        // transitionProgress = -1 means no transition is ongoing.
        private static SeekerBarrierColorController nextController;
        private static float transitionProgress = -1f;

        // the parameters for this seeker controller.
        private Color color;
        private Color particleColor;
        private float transparency;
        private float particleTransparency;
        private float particleDirection;
        private bool persistent;

        public SeekerBarrierColorController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            color = Calc.HexToColor(data.Attr("color", "FFFFFF"));
            particleColor = Calc.HexToColor(data.Attr("particleColor", "FFFFFF"));
            transparency = data.Float("transparency", 0.15f);
            particleTransparency = data.Float("particleTransparency", 0.5f);
            particleDirection = data.Float("particleDirection", 0f);
            persistent = data.Bool("persistent");

            Add(new TransitionListener {
                OnIn = progress => transitionProgress = progress,
                OnOut = progress => transitionProgress = progress,
                OnInBegin = () => transitionProgress = 0f,
                OnInEnd = () => transitionProgress = -1f
            });

            // update session
            if (persistent) {
                MaxHelpingHandModule.Instance.Session.SeekerBarrierCurrentColors = new MaxHelpingHandSession.SeekerBarrierColorState() {
                    Color = data.Attr("color", "FFFFFF"),
                    ParticleColor = data.Attr("particleColor", "FFFFFF"),
                    Transparency = data.Float("transparency", 0.15f),
                    ParticleTransparency = data.Float("particleTransparency", 0.5f),
                    ParticleDirection = data.Float("particleDirection", 0f)
                };
            } else {
                MaxHelpingHandModule.Instance.Session.SeekerBarrierCurrentColors = null;
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // this is the controller for the next screen.
            nextController = this;

            // enable the hooks on barrier rendering.
            if (!seekerBarrierRendererHooked) {
                hookSeekerBarrierRenderer();
                seekerBarrierRendererHooked = true;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            // the "current" color controller is now the one from the next screen.
            controllerOnScreen = nextController;
            nextController = null;

            // the transition (if any) is over.
            transitionProgress = -1f;

            // if there is none, clean up the hooks.
            if (controllerOnScreen == null && seekerBarrierRendererHooked) {
                unhookSeekerBarrierRenderer();
                seekerBarrierRendererHooked = false;
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            // leaving level: forget about all controllers and clean up the hooks if present.
            controllerOnScreen = null;
            nextController = null;
            if (seekerBarrierRendererHooked) {
                unhookSeekerBarrierRenderer();
                seekerBarrierRendererHooked = false;
            };
        }

        public override void Update() {
            base.Update();

            if (transitionProgress == -1f && controllerOnScreen == null) {
                // no transition is ongoing.
                // if only nextController is defined, move it into controllerOnScreen.
                controllerOnScreen = nextController;
                nextController = null;
            }
        }

        private void hookSeekerBarrierRenderer() {
            IL.Celeste.SeekerBarrierRenderer.OnRenderBloom += hookBarrierColor;
            IL.Celeste.SeekerBarrierRenderer.Render += hookBarrierColor;
            IL.Celeste.SeekerBarrier.Render += hookParticleColors;
            On.Celeste.SeekerBarrier.Update += hookSeekerBarrierParticles;
        }

        private static void unhookSeekerBarrierRenderer() {
            IL.Celeste.SeekerBarrierRenderer.OnRenderBloom -= hookBarrierColor;
            IL.Celeste.SeekerBarrierRenderer.Render -= hookBarrierColor;
            IL.Celeste.SeekerBarrier.Render -= hookParticleColors;
            On.Celeste.SeekerBarrier.Update -= hookSeekerBarrierParticles;
        }

        private static void hookSeekerBarrierParticles(On.Celeste.SeekerBarrier.orig_Update orig, SeekerBarrier self) {
            // no need to account for screen transitions: particles are frozen during them.
            if (controllerOnScreen == null || controllerOnScreen.particleDirection == 0f) {
                // default settings: do nothing
                orig(self);
                return;
            }

            // save all particles
            DynData<SeekerBarrier> selfData = new DynData<SeekerBarrier>(self);
            List<Vector2> particles = new List<Vector2>(selfData.Get<List<Vector2>>("particles"));
            float[] speeds = selfData.Get<float[]>("speeds");

            // run vanilla code
            orig(self);

            // move particles again ourselves, except on the direction we want.
            for (int i = 0; i < particles.Count; i++) {
                // compute new position
                Vector2 newPosition = particles[i] + Vector2.UnitY.Rotate((float) (controllerOnScreen.particleDirection * Math.PI / 180)) * speeds[i % speeds.Length] * Engine.DeltaTime;

                // make sure it stays in bounds
                while (newPosition.X < 0) newPosition.X += self.Width;
                while (newPosition.Y < 0) newPosition.Y += self.Height;
                newPosition.X %= self.Width;
                newPosition.Y %= self.Height;

                // replace the particle position
                particles[i] = newPosition;
            }

            // replace them.
            selfData["particles"] = particles;
        }

        private static void hookBarrierColor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // replace colors (vanilla is white)...
            while (cursor.TryGotoNext(instr => instr.MatchCall<Color>("get_White"))) {
                Logger.Log("MaxHelpingHand/SeekerBarrierColorController", $"Injecting seeker barrier color at {cursor.Index} in IL for {il.Method.Name}");
                cursor.Next.OpCode = OpCodes.Call;
                cursor.Next.Operand = typeof(SeekerBarrierColorController).GetMethod("GetCurrentBarrierColor");
            }

            // reset the cursor...
            cursor.Index = 0;

            // ... and replace opacity (vanilla is 0.15).
            if (cursor.TryGotoNext(instr => instr.MatchLdcR4(0.15f))) {
                Logger.Log("MaxHelpingHand/SeekerBarrierColorController", $"Injecting seeker barrier transparency at {cursor.Index} in IL for {il.Method.Name}");
                cursor.Next.OpCode = OpCodes.Call;
                cursor.Next.Operand = typeof(SeekerBarrierColorController).GetMethod("GetCurrentBarrierTransparency");
            }
        }

        private static void hookParticleColors(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // replace colors (vanilla is white)...
            if (cursor.TryGotoNext(instr => instr.MatchCall<Color>("get_White"))) {
                Logger.Log("MaxHelpingHand/SeekerBarrierColorController", $"Injecting seeker barrier particle color at {cursor.Index} in IL for {il.Method.Name}");
                cursor.Next.OpCode = OpCodes.Call;
                cursor.Next.Operand = typeof(SeekerBarrierColorController).GetMethod("GetCurrentParticleColor");
            }

            // reset the cursor...
            cursor.Index = 0;

            // ... and replace opacity (vanilla is 0.5).
            if (cursor.TryGotoNext(instr => instr.MatchLdcR4(0.5f))) {
                Logger.Log("MaxHelpingHand/SeekerBarrierColorController", $"Injecting seeker barrier particle transparency at {cursor.Index} in IL for {il.Method.Name}");
                cursor.Next.OpCode = OpCodes.Call;
                cursor.Next.Operand = typeof(SeekerBarrierColorController).GetMethod("GetCurrentParticleTransparency");
            }
        }

        // those are called from the IL hooks:

        public static Color GetCurrentBarrierColor() {
            return getAndLerp(controller => controller.color, Color.White, Color.Lerp);
        }

        public static Color GetCurrentParticleColor() {
            return getAndLerp(controller => controller.particleColor, Color.White, Color.Lerp);
        }

        public static float GetCurrentBarrierTransparency() {
            return getAndLerp(controller => controller.transparency, 0.15f, MathHelper.Lerp);
        }

        public static float GetCurrentParticleTransparency() {
            return getAndLerp(controller => controller.particleTransparency, 0.5f, MathHelper.Lerp);
        }

        /// <summary>
        /// Gets a value from the active seeker barrier color controller(s), using the given getter, and the lerp function if we are transitioning
        /// between rooms. defaultValue is used when going from/to a room with no controller.
        /// </summary>
        private static T getAndLerp<T>(Func<SeekerBarrierColorController, T> valueGetter, T defaultValue, Func<T, T, float, T> lerp) {
            if (transitionProgress == -1f) {
                if (controllerOnScreen == null) {
                    // no transition is ongoing.
                    // if only nextController is defined, move it into controllerOnScreen.
                    controllerOnScreen = nextController;
                    nextController = null;
                }

                if (controllerOnScreen != null) {
                    return valueGetter(controllerOnScreen);
                } else {
                    return defaultValue;
                }
            } else {
                // get the value in the room we're coming from.
                T fromRoomValue;
                if (controllerOnScreen != null) {
                    fromRoomValue = valueGetter(controllerOnScreen);
                } else {
                    fromRoomValue = defaultValue;
                }

                // get the value in the room we're going to.
                T toRoomValue;
                if (nextController != null) {
                    toRoomValue = valueGetter(nextController);
                } else {
                    toRoomValue = defaultValue;
                }

                // transition smoothly between both.
                return lerp(fromRoomValue, toRoomValue, transitionProgress);
            }
        }
    }
}
