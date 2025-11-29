using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    file class RespawningJellyfishExt : RespawningJellyfishGeneric<RespawningJellyfish, Glider> {
        public RespawningJellyfishExt(RespawningJellyfish self, EntityData data, Action<Sprite> setupSpriteCallback, Func<Vector2> getSpeed, Action<Vector2> setSpeed) : base(self, data, setupSpriteCallback, getSpeed, setSpeed) {
        }
        protected override void onDestroy() => self.destroyed = true;
        protected override void onRespawn(bool bubble) {
            self.destroyed = false;
            self.bubble = bubble;
        }
        protected override Sprite getSprite() => self.sprite;
        protected override void jellySpritePlay(Glider self, string anim) => self.sprite.Play(anim);
    }

    [CustomEntity("MaxHelpingHand/RespawningJellyfish")]
    public class RespawningJellyfish : Glider {
        internal RespawningJellyfishGeneric<RespawningJellyfish, Glider> manager;

        public RespawningJellyfish(EntityData data, Vector2 offset) : base(data, offset) {
            manager = new RespawningJellyfishExt(this, data, sprite => {
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
    }
}
