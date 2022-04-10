using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ParallaxFadeSpeedController")]
    [Tracked]
    public class ParallaxFadeSpeedController : Entity {

        private static bool backdropHooked = false;

        private readonly float fadeTime;

        public ParallaxFadeSpeedController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            fadeTime = data.Float("fadeTime", 1f);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // enable the hook on backdrop updating.
            if (!backdropHooked) {
                backdropHooked = true;
                IL.Celeste.Parallax.Update += modBackdropUpdate;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            // disable the hook on backdrop updating.
            if (backdropHooked && scene.Tracker.CountEntities<ParallaxFadeSpeedController>() <= 1) {
                backdropHooked = false;
                IL.Celeste.Parallax.Update -= modBackdropUpdate;
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            // disable the hook on backdrop updating.
            if (backdropHooked) {
                backdropHooked = false;
                IL.Celeste.Parallax.Update -= modBackdropUpdate;
            }
        }

        private static void modBackdropUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(instr => instr.MatchLdfld<Parallax>("DoFadeIn"))
                && cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Engine>("get_DeltaTime"))) {

                Logger.Log("MaxHelpingHand/ParallaxFadeSpeedController", $"Injecting hook to change parallax fade speed at {cursor.Index} in IL for Backdrop.Update");

                cursor.EmitDelegate<Func<float, float>>(orig => {
                    ParallaxFadeSpeedController controller = Engine.Scene.Tracker.GetEntity<ParallaxFadeSpeedController>();
                    if (controller != null) {
                        return orig / controller.fadeTime;
                    }

                    return orig;
                });
            }
        }
    }
}
