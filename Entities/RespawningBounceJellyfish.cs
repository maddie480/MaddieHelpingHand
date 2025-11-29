using Celeste.Mod.BounceHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    file class RespawningBounceJellyfishExt : RespawningJellyfishGeneric<RespawningBounceJellyfish, BounceJellyfish> {
        public RespawningBounceJellyfishExt(RespawningBounceJellyfish self, EntityData data, Action<Sprite> setupSpriteCallback, Func<Vector2> getSpeed, Action<Vector2> setSpeed) : base(self, data, setupSpriteCallback, getSpeed, setSpeed) {
        }
        protected override void onDestroy() => self.destroyed = true;
        protected override void resetDashBufferTimer() => RespawningBounceJellyfish.bounceJellyDashBufferTimer.SetValue(self, 0f);
        protected override void onRespawn(bool bubble) {
            self.destroyed = false;
            RespawningBounceJellyfish.bounceJellyPlatform.SetValue(self, bubble);
        }
        protected override Sprite getSprite() => (Sprite) RespawningBounceJellyfish.bounceJellySprite.GetValue(self);
        protected override void jellySpritePlay(BounceJellyfish self, string anim) => RespawningBounceJellyfish.bounceJellySpritePlay.Invoke(self, new object[] { anim });
        protected override void jellyDashRefill(BounceJellyfish self) => self.refillDash();
    }
    public class RespawningBounceJellyfish : BounceJellyfish {
        internal static FieldInfo bounceJellyPlatform;
        internal static FieldInfo bounceJellyDashBufferTimer;
        internal static FieldInfo bounceJellySprite;
        internal static MethodInfo bounceJellySpritePlay;

        public static void LoadBounceHelper() {
            RespawningJellyfishGeneric<RespawningBounceJellyfish, BounceJellyfish>.Load();
            Everest.Events.Level.OnLoadEntity += onLoadEntity;
            On.Monocle.Tracker.Initialize += onTrackerInitialize;

            bounceJellyPlatform = typeof(BounceJellyfish).GetField("platform", BindingFlags.NonPublic | BindingFlags.Instance);
            bounceJellyDashBufferTimer = typeof(BounceJellyfish).GetField("dashBufferTimer", BindingFlags.NonPublic | BindingFlags.Instance);
            bounceJellySprite = typeof(BounceJellyfish).GetField("sprite", BindingFlags.NonPublic | BindingFlags.Instance);
            bounceJellySpritePlay = typeof(BounceJellyfish).GetMethod("spritePlay", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static void UnloadBounceHelper() {
            RespawningJellyfishGeneric<RespawningBounceJellyfish, BounceJellyfish>.Unload();
            Everest.Events.Level.OnLoadEntity -= onLoadEntity;
            On.Monocle.Tracker.Initialize -= onTrackerInitialize;

            bounceJellyPlatform = null;
        }

        private static bool onLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            if (entityData.Name == "MaxHelpingHand/RespawningBounceJellyfish") {
                level.Add(new RespawningBounceJellyfish(entityData, offset));
                return true;
            }

            return false;
        }

        private static void onTrackerInitialize(On.Monocle.Tracker.orig_Initialize orig) {
            orig();
            Tracker.TrackedEntityTypes[typeof(RespawningBounceJellyfish)] = new List<Type> { typeof(BounceJellyfish) };
        }


        internal RespawningJellyfishGeneric<RespawningBounceJellyfish, BounceJellyfish> manager;

        public RespawningBounceJellyfish(EntityData data, Vector2 offset) : base(data, offset) {
            manager = new RespawningBounceJellyfishExt(this, data, sprite => {
                foreach (string variant in new string[] { "blue", "red", "pink", "flash" }) {
                    string suffix = variant.Substring(0, 1).ToUpperInvariant();

                    sprite.AddLoop("idle" + suffix, variant + "/idle", 0.1f);
                    sprite.AddLoop("held" + suffix, variant + "/held", 0.1f);
                    sprite.Add("fall" + suffix, variant + "/fall", 0.06f, "fallLoop" + suffix);
                    sprite.AddLoop("fallLoop" + suffix, variant + "/fallLoop", 0.06f);
                    sprite.Add("death" + suffix, variant + "/death", 0.06f);
                    sprite.Add("respawn" + suffix, variant + "/respawn", 0.03f, "idle" + suffix);
                }

                sprite.Play("idleB");
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
