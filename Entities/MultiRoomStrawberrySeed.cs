using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Celeste.Mod;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/MultiRoomStrawberrySeed")]
    [Tracked]
    public class MultiRoomStrawberrySeed : StrawberrySeed {
        public static void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
            IL.Celeste.Player.ClimbHopBlockedCheck += onClimbHopBlockedCheck;
            On.Celeste.StrawberrySeed.ReturnRoutine += onStrawberrySeedReturnRoutine;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
            IL.Celeste.Player.ClimbHopBlockedCheck -= onClimbHopBlockedCheck;
            On.Celeste.StrawberrySeed.ReturnRoutine -= onStrawberrySeedReturnRoutine;
        }

        private static void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                Player player = self.Tracker.GetEntity<Player>();

                if (player != null) {
                    Vector2 seedPosition = player.Position;

                    // we have to restore collected strawberry seeds.
                    foreach (MaxHelpingHandSession.MultiRoomStrawberrySeedInfo sessionSeedInfo in MaxHelpingHandModule.Instance.Session.CollectedMultiRoomStrawberrySeeds) {
                        seedPosition += new Vector2(-12 * (int) player.Facing, -8f);

                        self.Add(new MultiRoomStrawberrySeed(player, seedPosition, sessionSeedInfo));
                    }
                }
            }
        }

        private static void onClimbHopBlockedCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Component>("get_Entity"), instr => instr.MatchIsinst<StrawberrySeed>())) {
                Logger.Log("MaxHelpingHand/MultiRoomStrawberrySeed", $"Disabling climb-hop block while holding multi-room seeds @ {cursor.Index} in Player.ClimbHopBlockedCheck");

                // isinst is misleading. It's actually the equivalent of "entity as StrawberrySeed" in C#.
                // so, if our seed is a multi-room strawberry seed, we want to return null to make the game think it isn't a StrawberrySeed.
                cursor.EmitDelegate<Func<StrawberrySeed, StrawberrySeed>>(strawberrySeed => {
                    if (strawberrySeed == null || !(strawberrySeed is MultiRoomStrawberrySeed || strawberrySeed is NonPoppingStrawberrySeed)) {
                        return strawberrySeed;
                    }
                    return null;
                });
            }
        }

        private static IEnumerator onStrawberrySeedReturnRoutine(On.Celeste.StrawberrySeed.orig_ReturnRoutine orig, StrawberrySeed self) {
            yield return new SwapImmediately(orig(self));

            // if the strawberry seed returned to another room, it should just disappear.
            if (self is MultiRoomStrawberrySeed strawberrySeed && (self.Scene as Level).Session.Level != strawberrySeed.startingRoom) {
                self.RemoveSelf();
            }
        }

        private DynData<StrawberrySeed> selfStrawberrySeed;

        private int index;
        public EntityID BerryID;
        private int seedCount;
        private bool displaySeedCount;

        private string startingRoom;

        private float canLoseTimerMirror;
        private Player player;
        private bool spawnedAsFollower = false;

        private string sprite;
        private bool ghost;

        public MultiRoomStrawberrySeed(Vector2 position, int index, bool ghost, string sprite, string ghostSprite) : base(null, position, index, ghost) {
            selfStrawberrySeed = new DynData<StrawberrySeed>(this);

            this.index = index;
            this.ghost = ghost;
            this.sprite = ghost ? ghostSprite : sprite;

            foreach (Component component in this) {
                if (component is PlayerCollider playerCollider) {
                    playerCollider.OnCollide = OnPlayer;
                }
                if (component is Follower follower) {
                    follower.ParentEntityID = EntityID.None;
                }
            }
        }

        public MultiRoomStrawberrySeed(EntityData data, Vector2 offset) : this(data.Position + offset, data.Int("index"),
            SaveData.Instance.CheckStrawberry(new EntityID(data.Attr("berryLevel"), data.Int("berryID"))),
            data.Attr("sprite", "MaxHelpingHand/miniberry/miniberry"), data.Attr("ghostSprite", "MaxHelpingHand/miniberry/ghostminiberry")) {

            BerryID = new EntityID(data.Attr("berryLevel"), data.Int("berryID"));
            seedCount = data.Int("seedCount");
            displaySeedCount = data.Bool("displaySeedCount");
        }

        private MultiRoomStrawberrySeed(Player player, Vector2 position, MaxHelpingHandSession.MultiRoomStrawberrySeedInfo sessionSeedInfo)
            : this(position, sessionSeedInfo.Index, SaveData.Instance.CheckStrawberry(sessionSeedInfo.BerryID), sessionSeedInfo.Sprite, sessionSeedInfo.Sprite) {

            BerryID = sessionSeedInfo.BerryID;
            startingRoom = sessionSeedInfo.StartingRoom;

            // the seed is collected right away.
            this.player = player;
            spawnedAsFollower = true;

            // set the starting point so that the berry seed can go back there if it pops.
            new DynData<StrawberrySeed>(this)["start"] = sessionSeedInfo.StartingPoint;
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (!spawnedAsFollower) {
                startingRoom = (scene as Level).Session.Level;

                if (SceneAs<Level>().Session.GetFlag("collected_seeds_of_" + BerryID.ToString())) {
                    // if all seeds for this berry were already collected (the berry was already formed), commit remove self.
                    RemoveSelf();
                } else {
                    // if the seed already follows the player, commit remove self.
                    foreach (MaxHelpingHandSession.MultiRoomStrawberrySeedInfo sessionSeedInfo in MaxHelpingHandModule.Instance.Session.CollectedMultiRoomStrawberrySeeds) {
                        if (sessionSeedInfo.Index == index && sessionSeedInfo.BerryID.ID == BerryID.ID && sessionSeedInfo.BerryID.Level == BerryID.Level) {
                            RemoveSelf();
                            break;
                        }
                    }
                }
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if ((ghost && sprite != "ghostberry/seed") || (!ghost && sprite != "strawberry/seed")) {
                // the sprite is non-default. replace it.
                Sprite vanillaSprite = selfStrawberrySeed.Get<Sprite>("sprite");

                // build the new sprite.
                MTexture frame0 = GFX.Game["collectables/" + sprite + "00"];
                MTexture frame1 = GFX.Game["collectables/" + sprite + "01"];

                Sprite modSprite = new Sprite(GFX.Game, sprite);
                modSprite.CenterOrigin();
                modSprite.Justify = new Vector2(0.5f, 0.5f);
                modSprite.AddLoop("idle", 0.1f, new MTexture[] {
                    frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame0,
                    frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame0, frame1
                });
                modSprite.AddLoop("noFlash", 0.1f, new MTexture[] { frame0 });

                // copy over the values from the vanilla sprite
                modSprite.Position = vanillaSprite.Position;
                modSprite.Color = vanillaSprite.Color;
                modSprite.OnFrameChange = vanillaSprite.OnFrameChange;
                modSprite.Play("idle");
                modSprite.SetAnimationFrame(vanillaSprite.CurrentAnimationFrame);

                // and replace it for good
                Remove(vanillaSprite);
                Add(modSprite);
                selfStrawberrySeed["sprite"] = modSprite;
            }

            if (spawnedAsFollower) {
                player.Leader.GainFollower(selfStrawberrySeed.Get<Follower>("follower"));
                canLoseTimerMirror = 0.25f;
                Collidable = false;
                Depth = -1000000;
                AddTag(Tags.Persistent);
            }
        }

        private void OnPlayer(Player player) {
            Audio.Play("event:/game/general/seed_touch", Position, "count", index);
            player.Leader.GainFollower(selfStrawberrySeed.Get<Follower>("follower"));
            canLoseTimerMirror = 0.25f;
            Collidable = false;
            Depth = -1000000;
            AddTag(Tags.Persistent);

            // Add the info for this berry seed to the session.
            MaxHelpingHandSession.MultiRoomStrawberrySeedInfo sessionSeedInfo = new MaxHelpingHandSession.MultiRoomStrawberrySeedInfo();
            sessionSeedInfo.Index = index;
            sessionSeedInfo.BerryID = BerryID;
            sessionSeedInfo.Sprite = sprite;
            sessionSeedInfo.StartingRoom = startingRoom;
            sessionSeedInfo.StartingPoint = new DynData<StrawberrySeed>(this).Get<Vector2>("start");
            MaxHelpingHandModule.Instance.Session.CollectedMultiRoomStrawberrySeeds.Add(sessionSeedInfo);

            if (displaySeedCount) {
                // look at all the player followers, and filter all the seeds that match our berry.
                List<StrawberrySeed> matchingSeeds = new List<StrawberrySeed>();
                foreach (Follower follower in player.Leader.Followers) {
                    if (follower.Entity is MultiRoomStrawberrySeed seed) {
                        if (seed.BerryID.Level == BerryID.Level && seed.BerryID.ID == BerryID.ID) {
                            matchingSeeds.Add(seed);
                        }
                    }
                }

                // display a counter over Madeline's head
                Scene.Remove(Scene.Tracker.GetEntities<MultiRoomStrawberryCounter>());
                Scene.Add(new MultiRoomStrawberryCounter(matchingSeeds.Count + "/" + seedCount));
            }
        }

        public override void Update() {
            base.Update();

            // be sure the canLoseTimer always has a positive value. we don't want the player to lose this berry seed.
            canLoseTimerMirror -= Engine.DeltaTime;
            if (canLoseTimerMirror < 1f) {
                canLoseTimerMirror = 1000f;
                selfStrawberrySeed["canLoseTimer"] = 1000f;
            }
        }
    }
}
