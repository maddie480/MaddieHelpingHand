using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ParallaxFadeSpeedController")]
    [Tracked]
    public class ParallaxFadeSpeedController : Entity {
        public static void Load() {
            IL.Celeste.Parallax.Update += modBackdropUpdate;
        }

        public static void Unload() {
            IL.Celeste.Parallax.Update -= modBackdropUpdate;
        }


        private static bool backdropHookEnabled = false;

        private readonly float fadeTime;

        public ParallaxFadeSpeedController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            fadeTime = data.Float("fadeTime", 1f);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // enable the hook on backdrop updating.
            backdropHookEnabled = true;
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            // disable the hook on backdrop updating.
            if (scene.Tracker.CountEntities<ParallaxFadeSpeedController>() <= 1) {
                backdropHookEnabled = false;
            }
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            // disable the hook on backdrop updating.
            backdropHookEnabled = false;
        }

        private static void modBackdropUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(instr => instr.MatchLdfld<Parallax>("DoFadeIn"))
                && cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Engine>("get_DeltaTime"))) {

                Logger.Log("MaxHelpingHand/ParallaxFadeSpeedController", $"Injecting hook to change parallax fade speed at {cursor.Index} in IL for Backdrop.Update");

                cursor.EmitDelegate<Func<float, float>>(orig => {
                    if (!backdropHookEnabled) return orig;

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
