using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/RespawningJellyfish")]
    public class RespawningJellyfish : Glider {
        public static void Load() {
            IL.Celeste.Glider.Update += modGliderUpdate;
        }

        public static void Unload() {
            IL.Celeste.Glider.Update -= modGliderUpdate;
        }

        private static ParticleType P_NotGlow;

        private DynData<Glider> self;

        private float respawnTime;
        private bool bubble;

        private Sprite sprite;

        private Vector2 initialPosition;
        private bool respawning;

        private bool shouldRespawn = true;

        public RespawningJellyfish(EntityData data, Vector2 offset) : base(data, offset) {
            if (P_NotGlow == null) {
                // P_NotGlow is a transparent particle.
                P_NotGlow = new ParticleType(P_Glow) {
                    Color = Color.Transparent,
                    Color2 = Color.Transparent
                };
            }

            respawnTime = data.Float("respawnTime");
            bubble = data.Bool("bubble");
            initialPosition = Position;
            respawning = false;

            // get the sprite, and replace it depending on the path in entity properties.
            self = new DynData<Glider>(this);
            sprite = self.Get<Sprite>("sprite");
            new DynData<Sprite>(sprite)["atlas"] = GFX.Game;
            sprite.Path = data.Attr("spriteDirectory", defaultValue: "objects/MaxHelpingHand/glider") + "/";
            sprite.Stop();
            sprite.ClearAnimations();
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.AddLoop("held", "held", 0.1f);
            sprite.Add("fall", "fall", 0.06f, "fallLoop");
            sprite.AddLoop("fallLoop", "fallLoop", 0.06f);
            sprite.Add("death", "death", 0.06f);
            sprite.Add("respawn", "respawn", 0.03f, "idle");
            sprite.Play("idle");

            // make the jelly go invisible when the death animation is done.
            sprite.OnFinish += anim => {
                if (anim == "death") {
                    Visible = false;
                }
            };

            // listen for transitions: if the jelly is carried to another screen, it should not respawn anymore.
            Add(new TransitionListener() {
                OnOutBegin = () => shouldRespawn = false
            });
        }

        public override void Update() {
            if (shouldRespawn && !respawning && Top + Speed.Y * Engine.DeltaTime > (SceneAs<Level>().Bounds.Bottom + 16)) {
                // the jellyfish glided off-screen.
                removeAndRespawn();
            }

            // if the jelly is invisible, "disable" the particles (actually make them invisible).
            ParticleType vanillaGlow = P_Glow;
            if (!Visible) P_Glow = P_NotGlow;

            base.Update();

            P_Glow = vanillaGlow;
        }

        private static void modGliderUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchNewobj<Coroutine>())) {
                Logger.Log("MaxHelpingHand/RespawningJellyfish", $"Replacing coroutine to make jellyfish respawn at {cursor.Index} in IL for Glider.Update");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Coroutine, Glider, Coroutine>>((orig, self) => {
                    if (self is RespawningJellyfish jelly) {
                        return new Coroutine(jelly.destroyThenRespawnRoutine());
                    }

                    return orig;
                });
            }
        }

        protected override void OnSquish(CollisionData data) {
            if (shouldRespawn) {
                if (!TrySquishWiggle(data)) {
                    // the jellyfish was squished.
                    removeAndRespawn();
                }
            } else {
                // vanilla behavior
                base.OnSquish(data);
            }
        }

        private void removeAndRespawn() {
            Collidable = false;
            Visible = false;
            self["destroyed"] = true;
            Add(new Coroutine(respawnRoutine()));
        }

        private IEnumerator destroyThenRespawnRoutine() {
            // do like vanilla, but instead of removing the jelly, wait then have it respawn.
            Audio.Play("event:/new_content/game/10_farewell/glider_emancipate", Position);
            sprite.Play("death");

            return respawnRoutine();
        }

        private IEnumerator respawnRoutine() {
            respawning = true;

            // wait for the respawn time
            yield return respawnTime;

            // then respawn at the initial position
            Visible = true;
            Position = initialPosition;
            Speed = Vector2.Zero;
            sprite.Play("respawn");

            yield return 0.24f;

            respawning = false;
            self["destroyed"] = false;
            self["bubble"] = bubble;
            Collidable = true;
        }
    }
}
