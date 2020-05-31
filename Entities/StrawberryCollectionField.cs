using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/StrawberryCollectionField")]
    class StrawberryCollectionField : Trigger {
        private float collectTimer = 0f;
        private bool delayBetweenBerries;
        private bool includeGoldens;

        public StrawberryCollectionField(EntityData data, Vector2 offset) : base(data, offset) {
            delayBetweenBerries = data.Bool("delayBetweenBerries");
            includeGoldens = data.Bool("includeGoldens");
        }

        public override void OnStay(Player player) {
            base.OnStay(player);

            if (collectTimer <= 0f) {
                ReadOnlyCollection<Type> berryTypes = StrawberryRegistry.GetBerryTypes();
                List<IStrawberry> followingBerries = new List<IStrawberry>();
                foreach (Follower follower in player.Leader.Followers) {
                    if (berryTypes.Contains(follower.Entity.GetType())
                        && follower.Entity is IStrawberry berry
                        && (includeGoldens || !isGolden(berry))) {

                        followingBerries.Add(berry);
                    }
                }
                foreach (IStrawberry berry in followingBerries) {
                    berry.OnCollect();

                    if (delayBetweenBerries) {
                        collectTimer = 0.3f;
                        break;
                    }
                }
            }
        }

        private bool isGolden(IStrawberry berry) {
            if (berry.GetType() == typeof(Strawberry)) {
                // vanilla berries are goldens if... they are goldens.
                return (berry as Strawberry).Golden;
            } else {
                // mod berries are "goldens" if they block normal collection.
                return StrawberryRegistry.GetRegisteredBerries().ToList()
                    .Find(berryDef => berryDef.berryClass == berry.GetType())
                    .blocksNormalCollection;
            }
        }

        public override void Update() {
            base.Update();

            if (delayBetweenBerries) {
                collectTimer -= Engine.DeltaTime;
            }
        }
    }
}
