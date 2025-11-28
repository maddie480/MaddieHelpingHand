using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/RespawningJellyfish")]
    public class RespawningJellyfish : Glider {
        private RespawningJellyfishGeneric<RespawningJellyfish, Glider> manager;

        public RespawningJellyfish(EntityData data, Vector2 offset) : base(data, offset) {
            manager = new RespawningJellyfishGeneric<RespawningJellyfish, Glider>(this, data, sprite => {
                sprite.AddLoop("idle", "idle", 0.1f);
                sprite.AddLoop("held", "held", 0.1f);
                sprite.Add("fall", "fall", 0.06f, "fallLoop");
                sprite.AddLoop("fallLoop", "fallLoop", 0.06f);
                sprite.Add("death", "death", 0.06f);
                sprite.Add("respawn", "respawn", 0.03f, "idle");

                sprite.Play("idle");
            }, () => Speed, speed => Speed = speed);
        }

        public override void Update() {
            manager.Update(base.Update);
        }

        public override void OnSquish(CollisionData data) {
            manager.OnSquish(base.OnSquish, data);
        }

        private IEnumerator destroyThenRespawnRoutine() {
            return manager.destroyThenRespawnRoutine();
        }

        private void spritePlay(string name) {
            manager.sprite.Play(name);
        }
    }
}
