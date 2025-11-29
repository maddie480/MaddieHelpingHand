using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    public abstract class RespawningJellyfishGeneric<RespawningType, BaseType> where RespawningType : Actor, BaseType where BaseType : Actor {
        private static ILHook hookGliderUpdate = null;

        public static void Load() {
            hookGliderUpdate = new ILHook(typeof(BaseType).GetMethod("Update"), modGliderUpdate);
        }

        public static void Unload() {
            hookGliderUpdate?.Dispose();
            hookGliderUpdate = null;
        }

        private static ParticleType P_NotGlow;

        protected RespawningType self;

        private float respawnTime;
        private bool bubble;

        internal Sprite sprite;

        private Vector2 initialPosition;
        private bool respawning;

        private bool shouldRespawn = true;

        private Func<Vector2> getSpeed;
        private Action<Vector2> setSpeed;

        public RespawningJellyfishGeneric(RespawningType self, EntityData data, Action<Sprite> setupSpriteCallback, Func<Vector2> getSpeed, Action<Vector2> setSpeed) {
            this.self = self;
            this.getSpeed = getSpeed;
            this.setSpeed = setSpeed;

            if (P_NotGlow == null) {
                // P_NotGlow is a transparent particle.
                P_NotGlow = new ParticleType(Glider.P_Glow) {
                    Color = Color.Transparent,
                    Color2 = Color.Transparent
                };
            }

            respawnTime = data.Float("respawnTime");
            bubble = data.Bool("bubble") || data.Bool("platform");
            initialPosition = self.Position;
            respawning = false;

            // get the sprite, and replace it depending on the path in entity properties.
            sprite = getSprite();
            sprite.atlas = GFX.Game;
            sprite.Path = data.Attr("spriteDirectory", defaultValue: "objects/MaxHelpingHand/glider") + "/";
            sprite.Stop();
            sprite.ClearAnimations();
            setupSpriteCallback(sprite);

            // make the jelly go invisible when the death animation is done.
            sprite.OnFinish += anim => {
                if (anim == "death") {
                    self.Visible = false;
                }
            };

            // listen for transitions: if the jelly is carried to another screen, it should not respawn anymore.
            self.Add(new TransitionListener() {
                OnOutBegin = () => shouldRespawn = false
            });
        }

        public void Update(Action baseUpdate) {
            if (shouldRespawn && !respawning && self.Top + getSpeed().Y * Engine.DeltaTime > (self.SceneAs<Level>().Bounds.Bottom + 16)) {
                // the jellyfish glided off-screen.
                removeAndRespawn();
            }

            // if the jelly is invisible, "disable" the particles (actually make them invisible).
            ParticleType vanillaGlow = Glider.P_Glow;
            if (!self.Visible) Glider.P_Glow = P_NotGlow;

            baseUpdate();

            Glider.P_Glow = vanillaGlow;
        }

        private static void modGliderUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchNewobj<Coroutine>())) {
                Logger.Log("MaxHelpingHand/RespawningJellyfish", $"Replacing coroutine to make jellyfish respawn at {cursor.Index} in IL for {cursor.Method.FullName}");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Coroutine, Entity, Coroutine>>(swapCoroutine);
            }
        }

        private static Coroutine swapCoroutine(Coroutine orig, Entity self) {
            if (self.GetType().Name == "RespawningBounceJellyfish") {
                IEnumerator routine = destroyThenRespawnCoroutineBounce(self);
                if (routine != null) return new Coroutine(routine);
            }
            if (self is RespawningJellyfish f) {
                return new Coroutine(f.manager.destroyThenRespawnRoutine());
            }

            return orig;
        }

        private static IEnumerator destroyThenRespawnCoroutineBounce(Entity self) {
            // in its own method because resolving RespawningBounceJellyfish requires Bounce Helper
            if (self is RespawningBounceJellyfish f) {
                return f.manager.destroyThenRespawnRoutine();
            }
            return null;
        }

        public void OnSquish(Action<CollisionData> baseOnSquish, CollisionData data) {
            if (shouldRespawn) {
                if (!self.TrySquishWiggle(data)) {
                    // the jellyfish was squished.
                    removeAndRespawn();
                }
            } else {
                // vanilla behavior
                baseOnSquish(data);
            }
        }

        private void removeAndRespawn() {
            self.Collidable = false;
            self.Visible = false;
            self.Add(new Coroutine(respawnRoutine()));
            onDestroy();
        }

        internal IEnumerator destroyThenRespawnRoutine() {
            // do like vanilla, but instead of removing the jelly, wait then have it respawn.
            Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", self.Position);
            jellySpritePlay(self, "death");

            return respawnRoutine();
        }

        private IEnumerator respawnRoutine() {
            respawning = true;

            // wait for the respawn time
            yield return respawnTime;

            // then respawn at the initial position
            self.Visible = true;
            self.Position = initialPosition;
            setSpeed(Vector2.Zero);
            jellySpritePlay(self, "respawn");

            // refill dashes and cancel ongoing dashes (bounce jellies only)
            jellyDashRefill(self);

            resetDashBufferTimer();

            yield return 0.24f;

            respawning = false;
            self.Collidable = true;
            onRespawn(bubble);
        }

        protected abstract void onDestroy();
        protected virtual void resetDashBufferTimer() { }
        protected abstract void onRespawn(bool bubble);
        protected abstract Sprite getSprite();
        protected abstract void jellySpritePlay(BaseType self, string anim);
        protected virtual void jellyDashRefill(BaseType self) { }

    }
}
