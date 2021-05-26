using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/RespawningJellyfish")]
    public class RespawningJellyfish : Glider {
        private DynData<Glider> self;

        private float respawnTime;
        private bool bubble;

        private Sprite sprite;

        private Vector2 initialPosition;
        private bool respawning;

        private bool shouldRespawn = true;

        public RespawningJellyfish(EntityData data, Vector2 offset) : base(data, offset) {
            respawnTime = data.Float("respawnTime");
            bubble = data.Bool("bubble");
            initialPosition = Position;
            respawning = false;

            // get the sprite, and give it a respawn animation.
            self = new DynData<Glider>(this);
            sprite = self.Get<Sprite>("sprite");
            new DynData<Sprite>(sprite)["atlas"] = GFX.Game;
            sprite.Add("respawn", "objects/MaxHelpingHand/glider/respawn", 0.03f, "idle");

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

            base.Update();

            if (shouldRespawn && !respawning && self.Get<bool>("destroyed")) {
                // replace the vanilla destroy routine with our custom one.
                foreach (Component component in this) {
                    if (component is Coroutine) {
                        Remove(component);
                        Add(new Coroutine(destroyThenRespawnRoutine()));
                        break;
                    }
                }
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
