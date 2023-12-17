using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/RespawningJellyfish")]
    public class RespawningJellyfish : Glider {
        private RespawningJellyfishGeneric<RespawningJellyfish, Glider> manager;

        public RespawningJellyfish(EntityData data, Vector2 offset) : base(data, offset) {
            manager = new RespawningJellyfishGeneric<RespawningJellyfish, Glider>(this, data, () => Speed, speed => Speed = speed);
        }

        public override void Update() {
            manager.Update(base.Update);
        }

        protected override void OnSquish(CollisionData data) {
            manager.OnSquish(base.OnSquish, data);
        }

        private IEnumerator destroyThenRespawnRoutine() {
            return manager.destroyThenRespawnRoutine();
        }
    }
}
