using Celeste.Mod.BounceHelper;
using Microsoft.Xna.Framework;
using System.Collections;
using Monocle;
using System.Collections.Generic;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    public class RespawningBounceJellyfish : BounceJellyfish {
        public static void LoadBounceHelper() {
            RespawningJellyfishGeneric<RespawningBounceJellyfish, BounceJellyfish>.Load();
            Everest.Events.Level.OnLoadEntity += onLoadEntity;
            On.Monocle.Tracker.Initialize += onTrackerInitialize;
        }

        public static void UnloadBounceHelper() {
            RespawningJellyfishGeneric<RespawningBounceJellyfish, BounceJellyfish>.Unload();
            Everest.Events.Level.OnLoadEntity -= onLoadEntity;
            On.Monocle.Tracker.Initialize -= onTrackerInitialize;
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


        private RespawningJellyfishGeneric<RespawningBounceJellyfish, BounceJellyfish> manager;

        public RespawningBounceJellyfish(EntityData data, Vector2 offset) : base(data, offset) {
            manager = new RespawningJellyfishGeneric<RespawningBounceJellyfish, BounceJellyfish>(this, data, () => Speed, speed => Speed = speed);
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
