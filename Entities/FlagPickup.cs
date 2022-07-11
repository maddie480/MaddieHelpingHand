using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/FlagPickup")]
    public class FlagPickup : Entity {
        public static void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
        }

        private static void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);

            if (playerIntro != Player.IntroTypes.Transition) {
                Player player = self.Tracker.GetEntity<Player>();

                if (player != null) {
                    Vector2 spawnPosition = player.Position;

                    // we have to restore collected flag pickups.
                    foreach (MaxHelpingHandSession.FlagPickupInfo pickupInfo in MaxHelpingHandModule.Instance.Session.PickedUpFlagPickups) {
                        spawnPosition += new Vector2(-12 * (int) player.Facing, -8f);
                        self.Add(new FlagPickup(pickupInfo, player, spawnPosition));
                    }
                }
            }
        }

        private readonly string appearOnFlag;
        private readonly string flagOnPickup;
        private readonly string collectFlag;
        private readonly string spriteName;
        private readonly EntityID id;

        private TalkComponent talker = null;

        private Follower follower = null;
        private float collectTimer = 0f;

        private MaxHelpingHandSession.FlagPickupInfo pickupInfo;

        private Sprite sprite;

        // constructor used when the pickup is placed on the map
        public FlagPickup(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset) {
            this.id = id;

            appearOnFlag = data.Attr("appearOnFlag");
            flagOnPickup = data.Attr("flagOnPickup");
            collectFlag = data.Attr("collectFlag");
            spriteName = data.Attr("spriteName");

            // create the activation zone for the talk button
            Add(talker = new TalkComponent(new Rectangle(-16, -8, 32, 16), new Vector2(0, -4f), onPickup));
        }

        // constructor used when the player is respawning while having collected the pickup
        private FlagPickup(MaxHelpingHandSession.FlagPickupInfo savedInfo, Player player, Vector2 position) : base(position) {
            collectFlag = savedInfo.CollectFlag;
            spriteName = savedInfo.Sprite;

            pickupInfo = savedInfo;

            // spawn the follower right away!
            Add(follower = new Follower());
            player.Leader.GainFollower(follower);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (!string.IsNullOrEmpty(appearOnFlag) && !SceneAs<Level>().Session.GetFlag(appearOnFlag)) {
                RemoveSelf();
            }

            Depth = 1;

            // initialize the appropriate visuals
            Add(sprite = GFX.SpriteBank.Create(spriteName));
            sprite.Play(follower == null ? "still" : "following");

            // make sure the entity vanishes whenever the collect animation is done.
            sprite.OnFinish = anim => {
                if (anim == "collect") {
                    RemoveSelf();
                }
            };
        }

        public override void Update() {
            base.Update();

            if (follower != null) {
                // the pickup is following the player, we should check if it should get collected.
                if (!string.IsNullOrEmpty(collectFlag)) {
                    // trigger the collect whenever the flag is active...
                    if (SceneAs<Level>().Session.GetFlag(collectFlag)) {
                        onCollect();
                    }
                } else {
                    // ... and if there is no flag, mimic strawberries.
                    Player player = Scene.Tracker.GetEntity<Player>();
                    if (player != null && player.OnSafeGround) {
                        collectTimer += Engine.DeltaTime;
                        if (collectTimer > 0.15f) {
                            onCollect();
                        }
                    } else {
                        collectTimer = 0f;
                    }
                }
            }
        }

        private void onPickup(Player player) {
            // switch the sprite
            sprite.Play("following");

            // start following the player
            Add(follower = new Follower());
            SceneAs<Level>().OnEndOfFrame += () => player.Leader.GainFollower(follower);

            // there is no reason to interact with the pickup anymore
            talker.RemoveSelf();
            talker = null;

            // switch from being spawned as part of the map to being spawned with the player
            SceneAs<Level>().Session.DoNotLoad.Add(id);
            MaxHelpingHandModule.Instance.Session.PickedUpFlagPickups.Add(pickupInfo = new MaxHelpingHandSession.FlagPickupInfo() {
                Sprite = spriteName,
                CollectFlag = collectFlag
            });

            // raise the flag on pickup
            if (!string.IsNullOrEmpty(flagOnPickup)) {
                SceneAs<Level>().Session.SetFlag(flagOnPickup);
            }
        }

        private void onCollect() {
            // stop following the player
            follower.Leader?.LoseFollower(follower);
            follower.RemoveSelf();
            follower = null;

            // remove ourselves from session to avoid respawning the next time the player dies
            MaxHelpingHandModule.Instance.Session.PickedUpFlagPickups.Remove(pickupInfo);

            // play the collect animation to disappear!
            sprite.Play("collect");

            // play a sound as well
            Audio.Play("event:/game/general/seed_touch", Position);
        }
    }
}
