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
    internal static class RespawningJellyfishCache {
        internal static MethodInfo trySquishWiggle = typeof(Actor).GetMethod("TrySquishWiggle", BindingFlags.NonPublic | BindingFlags.Instance);
    }
    public class RespawningJellyfishGeneric<T, U> where T : Actor, U where U : Actor {
        private static ILHook hookGliderUpdate = null;

        public static void Load() {
            hookGliderUpdate = new ILHook(typeof(U).GetMethod("Update"), modGliderUpdate);
        }

        public static void Unload() {
            hookGliderUpdate?.Dispose();
            hookGliderUpdate = null;
        }

        private static ParticleType P_NotGlow;

        private T self;
        private DynData<U> selfData;

        private float respawnTime;
        private bool bubble;

        private Sprite sprite;

        private Vector2 initialPosition;
        private bool respawning;

        private bool shouldRespawn = true;

        private Func<Vector2> getSpeed;
        private Action<Vector2> setSpeed;

        public RespawningJellyfishGeneric(T self, EntityData data, Func<Vector2> getSpeed, Action<Vector2> setSpeed) {
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
            selfData = new DynData<U>(self);
            sprite = selfData.Get<Sprite>("sprite");
            new DynData<Sprite>(sprite)["atlas"] = GFX.Game;
            sprite.Path = data.Attr("spriteDirectory", defaultValue: "objects/MaxHelpingHand/glider") + "/";
            sprite.Stop();
            sprite.ClearAnimations();
            foreach (string suffix in new string[] { "", "B", "R", "RH", "P", "PH", "F" }) {
                sprite.AddLoop("idle" + suffix, "idle", 0.1f);
                sprite.AddLoop("held" + suffix, "held", 0.1f);
                sprite.Add("fall" + suffix, "fall", 0.06f, "fallLoop" + suffix);
                sprite.AddLoop("fallLoop" + suffix, "fallLoop", 0.06f);
                sprite.Add("death" + suffix, "death", 0.06f);
                sprite.Add("respawn" + suffix, "respawn", 0.03f, "idle" + suffix);
            }
            sprite.Play("idle");

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
                MethodInfo method = typeof(T).GetMethod("destroyThenRespawnRoutine", BindingFlags.NonPublic | BindingFlags.Instance);
                Logger.Log("MaxHelpingHand/RespawningJellyfish", $"Replacing coroutine to make jellyfish respawn at {cursor.Index} in IL for {cursor.Method.FullName}, calling {method.DeclaringType.FullName}.{method.Name}");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Coroutine, Glider, Coroutine>>((orig, self) => {
                    if (self is T) {
                        return new Coroutine((IEnumerator) method.Invoke(self, new object[0]));
                    }

                    return orig;
                });
            }
        }

        public void OnSquish(Action<CollisionData> baseOnSquish, CollisionData data) {
            if (shouldRespawn) {
                if (!((bool) RespawningJellyfishCache.trySquishWiggle.Invoke(self, new object[] { data }))) {
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
            sprite.Play("death");

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
            sprite.Play("respawn");

            yield return 0.24f;

            respawning = false;
            selfData["destroyed"] = false;
            selfData["bubble"] = bubble;
            selfData["platform"] = bubble;
            self.Collidable = true;
        }
    }
}
