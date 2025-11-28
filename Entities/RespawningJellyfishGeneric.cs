using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    public class RespawningJellyfishGeneric<RespawningType, BaseType> where RespawningType : Actor, BaseType where BaseType : Actor {
        private static ILHook hookGliderUpdate = null;
        private static MethodInfo jellyfishSpritePlay = typeof(BaseType).GetMethod("spritePlay", BindingFlags.NonPublic | BindingFlags.Instance)
            ?? typeof(RespawningType).GetMethod("spritePlay", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo jellyDashRefill = typeof(BaseType).GetMethod("refillDash", BindingFlags.Public | BindingFlags.Instance);
        private static MethodInfo trySquishWiggle = typeof(Actor).GetMethod("TrySquishWiggle", BindingFlags.NonPublic | BindingFlags.Instance,
            null, new Type[] { typeof(CollisionData) }, null);
        private static MethodInfo destroyThenRespawnRoutineRef =
            typeof(RespawningType).GetMethod("destroyThenRespawnRoutine", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Load() {
            hookGliderUpdate = new ILHook(typeof(BaseType).GetMethod("Update"), modGliderUpdate);
        }

        public static void Unload() {
            hookGliderUpdate?.Dispose();
            hookGliderUpdate = null;
        }

        private static ParticleType P_NotGlow;

        private RespawningType self;
        private DynData<BaseType> selfData;

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
            selfData = new DynData<BaseType>(self);
            sprite = selfData.Get<Sprite>("sprite");
            new DynData<Sprite>(sprite)["atlas"] = GFX.Game;
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
                cursor.EmitDelegate<Func<Coroutine, Glider, Coroutine>>(swapCoroutine);
            }
        }

        private static Coroutine swapCoroutine(Coroutine orig, Glider self) {
            if (self is RespawningType) {
                return new Coroutine((IEnumerator) destroyThenRespawnRoutineRef.Invoke(self, new object[0]));
            }

            return orig;
        }

        public void OnSquish(Action<CollisionData> baseOnSquish, CollisionData data) {
            if (shouldRespawn) {
                if (!((bool) trySquishWiggle.Invoke(self, new object[] { data }))) {
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
            selfData["destroyed"] = true;
            self.Add(new Coroutine(respawnRoutine()));
        }

        internal IEnumerator destroyThenRespawnRoutine() {
            // do like vanilla, but instead of removing the jelly, wait then have it respawn.
            Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", self.Position);
            jellyfishSpritePlay.Invoke(self, new object[] { "death" });

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
            jellyfishSpritePlay.Invoke(self, new object[] { "respawn" });

            // refill dashes and cancel ongoing dashes (bounce jellies only)
            jellyDashRefill?.Invoke(self, new object[] { -1 });
            selfData["dashBufferTimer"] = 0f;

            yield return 0.24f;

            respawning = false;
            selfData["destroyed"] = false;
            selfData["bubble"] = bubble;
            selfData["platform"] = bubble;
            self.Collidable = true;
        }
    }
}
