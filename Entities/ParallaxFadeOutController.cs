using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// A controller that make FadeIn parallax backdrops fade out as well.
    /// </summary>
    [CustomEntity("MaxHelpingHand/ParallaxFadeOutController")]
    [Tracked]
    public class ParallaxFadeOutController : Entity {
        public static void Load() {
            IL.Celeste.BackdropRenderer.Render += onBackdropRender;
        }

        public static void Unload() {
            IL.Celeste.BackdropRenderer.Render -= onBackdropRender;
        }

        private static bool backdropRendererHookEnabled = false;

        public ParallaxFadeOutController(EntityData data, Vector2 offset) : base(data.Position + offset) { }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // enable the hook on backdrop rendering.
            backdropRendererHookEnabled = true;
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            // disable the hook on backdrop rendering.
            if (scene.Tracker.CountEntities<ParallaxFadeOutController>() <= 1) {
                backdropRendererHookEnabled = false;
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            // disable the hook on backdrop rendering.
            backdropRendererHookEnabled = false;
        }

        private static void onBackdropRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(instr => instr.MatchLdfld<Backdrop>("Visible"))) {
                Logger.Log("MaxHelpingHand/ParallaxFadeOutController", $"Injecting hook to make backdrops fade out at {cursor.Index} in IL for BackdropRenderer.Render");

                cursor.Emit(OpCodes.Dup);
                cursor.Index++;
                cursor.EmitDelegate<Func<Backdrop, bool, bool>>(modIsVisible);
            }
        }

        private static bool modIsVisible(Backdrop backdrop, bool orig) {
            if (!backdropRendererHookEnabled) return orig;

            // force the game into rendering parallax backdrop that have fadeIn > 0 even if not visible.
            return orig || IsParallaxVisible(backdrop);
        }

        public static bool IsParallaxVisible(Backdrop backdrop) {
            return Engine.Scene.TimeActive > 1f && backdrop is Parallax parallax && parallax.DoFadeIn && parallax.fadeIn > 0f;
        }
    }
}
