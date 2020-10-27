using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/MultiRoomStrawberry")]
    [RegisterStrawberry(true, false)]
    class MultiRoomStrawberry : Strawberry {
        private int seedCount;
        private EntityID id;

        public MultiRoomStrawberry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {
            seedCount = data.Int("seedCount");
            id = gid;
        }

        public override void Added(Scene scene) {
            // trick the berry into thinking it has vanilla seeds, so that it doesn't appear right away
            StrawberrySeed dummySeed = new StrawberrySeed(this, Vector2.Zero, 0, false);
            Seeds = new List<StrawberrySeed> { dummySeed };

            base.Added(scene);

            scene.Remove(dummySeed);
            Seeds = null;
        }

        public override void Update() {
            base.Update();

            if (WaitingOnSeeds) {
                Player player = Scene.Tracker.GetEntity<Player>();
                if (player != null) {
                    // look at all the player followers, and filter all the seeds that match our berry.
                    List<StrawberrySeed> matchingSeeds = new List<StrawberrySeed>();
                    foreach (Follower follower in player.Leader.Followers) {
                        if (follower.Entity is MultiRoomStrawberrySeed seed) {
                            if (seed.BerryID.Level == id.Level && seed.BerryID.ID == id.ID) {
                                matchingSeeds.Add(seed);
                            }
                        }
                    }

                    if (matchingSeeds.Count >= seedCount) {
                        // all seeds have been gathered! associate the berry and the seeds, then trigger the cutscene.
                        Seeds = matchingSeeds;
                        foreach (StrawberrySeed seed in matchingSeeds) {
                            seed.Strawberry = this;
                        }

                        // build the "seed merging" cutscene with a transition listener to prevent it from breaking the game if the player transitions out before time is frozen.
                        CutsceneEntity seedsCutscene = new CSGEN_StrawberrySeeds(this);
                        seedsCutscene.Add(new TransitionListener() {
                            OnOutBegin = () => SceneAs<Level>().SkipCutscene()
                        });
                        Scene.Add(seedsCutscene);

                        // also clean up the session, since the seeds are now gone.
                        List<MaxHelpingHandSession.MultiRoomStrawberrySeedInfo> seedList = MaxHelpingHandModule.Instance.Session.CollectedMultiRoomStrawberrySeeds;
                        for (int i = 0; i < seedList.Count; i++) {
                            if (seedList[i].BerryID.Level == id.Level && seedList[i].BerryID.ID == id.ID) {
                                seedList.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            }
        }
    }
}
