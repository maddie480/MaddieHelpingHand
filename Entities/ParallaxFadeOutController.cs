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
    class ParallaxFadeOutController : Entity {

        private static bool backdropRendererHooked = false;

        public ParallaxFadeOutController(EntityData data, Vector2 offset) : base(data.Position + offset) { }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // enable the hook on backdrop rendering.
            if (!backdropRendererHooked) {
                backdropRendererHooked = true;
                IL.Celeste.BackdropRenderer.Render += onBackdropRender;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            // disable the hook on backdrop rendering.
            if (backdropRendererHooked && scene.Tracker.CountEntities<ParallaxFadeOutController>() <= 1) {
                backdropRendererHooked = false;
                IL.Celeste.BackdropRenderer.Render -= onBackdropRender;
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            // disable the hook on backdrop rendering.
            if (backdropRendererHooked) {
                backdropRendererHooked = false;
                IL.Celeste.BackdropRenderer.Render -= onBackdropRender;
            }
        }

        private static void onBackdropRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(instr => instr.MatchLdfld<Backdrop>("Visible"))) {
                Logger.Log("MaxHelpingHand/ParallaxFadeOutController", $"Injecting hook to make backdrops fade out at {cursor.Index} in IL for BackdropRenderer.Render");

                cursor.Emit(OpCodes.Dup);
                cursor.Index++;
                cursor.EmitDelegate<Func<Backdrop, bool, bool>>((backdrop, orig) => {
                    // force the game into rendering parallax backdrop that have fadeIn > 0 even if not visible.
                    return orig || (Engine.Scene.TimeActive > 1f && backdrop is Parallax parallax && parallax.DoFadeIn && new DynData<Parallax>(parallax).Get<float>("fadeIn") > 0f);
                });
            }
        }
    }
}
