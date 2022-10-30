using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomizableGlassFallingBlock")]
    public class CustomizableGlassFallingBlock : CustomizableGlassBlock {
        private readonly bool climbFall;
        private bool triggered = false;

        public CustomizableGlassFallingBlock(EntityData data, Vector2 offset) : base(data, offset) {
            climbFall = data.Bool("climbFall");
            Safe = false;
            Add(new Coroutine(Sequence()));
        }

        public override void OnShake(Vector2 amount) {
            base.OnShake(amount);
            ShakeVector += amount;
        }

        public override void OnStaticMoverTrigger(StaticMover sm) {
            triggered = true;
        }

        private bool playerFallCheck() {
            if (climbFall) {
                return HasPlayerRider();
            }
            return HasPlayerOnTop();
        }

        private bool playerWaitCheck() {
            if (triggered) {
                return true;
            }
            if (playerFallCheck()) {
                return true;
            }
            if (climbFall) {
                return CollideCheck<Player>(Position - Vector2.UnitX) || CollideCheck<Player>(Position + Vector2.UnitX);
            }
            return false;
        }

        private IEnumerator Sequence() {
            while (!triggered && !playerFallCheck()) {
                yield return null;
            }

            while (true) {
                // shake
                Audio.Play("event:/game/general/fallblock_shake", Center);
                StartShaking();
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

                // wait for a while before switching to falling
                yield return 0.2f;

                float timer = 0.4f;
                while (timer > 0f && playerWaitCheck()) {
                    yield return null;
                    timer -= Engine.DeltaTime;
                }
                StopShaking();

                // emit particles
                for (int i = 2; i < Width; i += 4) {
                    if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f))) {
                        SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustA, 2, new Vector2(X + i, Y), Vector2.One * 4f, (float) Math.PI / 2f);
                    }
                    SceneAs<Level>().Particles.Emit(FallingBlock.P_FallDustB, 2, new Vector2(X + i, Y), Vector2.One * 4f);
                }

                float speed = 0f;
                while (true) {
                    // fall
                    Level level = SceneAs<Level>();
                    speed = Calc.Approach(speed, 160f, 500f * Engine.DeltaTime);
                    if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true)) {
                        break;
                    }

                    // handle falling offscreen
                    if (Top > (level.Bounds.Bottom + 16) || (Top > (level.Bounds.Bottom - 1) && CollideCheck<Solid>(Position + new Vector2(0f, 1f)))) {
                        Collidable = (Visible = false);
                        yield return 0.2f;
                        if (level.Session.MapData.CanTransitionTo(level, new Vector2(Center.X, Bottom + 12f))) {
                            yield return 0.2f;
                            SceneAs<Level>().Shake();
                            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                        }
                        RemoveSelf();
                        DestroyStaticMovers();
                        yield break;
                    }
                    yield return null;
                }

                // impact with the ground
                Audio.Play("event:/game/general/fallblock_impact", BottomCenter);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                SceneAs<Level>().DirectionalShake(Vector2.UnitY, 0.3f);
                StartShaking();
                landParticles();
                yield return 0.2f;

                StopShaking();

                // solid tiles aren't going anywhere...
                if (CollideCheck<SolidTiles>(Position + new Vector2(0f, 1f))) {
                    break;
                }
                // ... but if we landed on a platform, it might go away.
                while (CollideCheck<Platform>(Position + new Vector2(0f, 1f))) {
                    yield return 0.1f;
                }
            }

            Safe = true;
        }

        private void landParticles() {
            for (int i = 2; i <= Width; i += 4) {
                if (Scene.CollideCheck<Solid>(BottomLeft + new Vector2(i, 3f))) {
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_FallDustA, 1, new Vector2(X + i, Bottom), Vector2.One * 4f, -(float) Math.PI / 2f);
                    float direction = (i >= Width / 2f) ? 0f : ((float) Math.PI);
                    SceneAs<Level>().ParticlesFG.Emit(FallingBlock.P_LandDust, 1, new Vector2(X + i, Bottom), Vector2.One * 4f, direction);
                }
            }
        }
    }
}
