using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/SpinnerBreakingBall")]
    public class SpinnerBreakingBall : TheoCrystal {
        public static void Load() {
            On.Celeste.TheoCrystal.Die += onTheoCrystalDie;
        }

        public static void Unload() {
            On.Celeste.TheoCrystal.Die -= onTheoCrystalDie;
        }

        private readonly CrystalColor color;
        private readonly EntityID entityID;

        private bool floating;

        private Dictionary<CrystalStaticSpinner, HashSet<CrystalStaticSpinner>> spinnerNeighbors;
        private HashSet<CrystalStaticSpinner> shatteredSpinners = new HashSet<CrystalStaticSpinner>();

        public SpinnerBreakingBall(EntityData data, Vector2 offset, EntityID entityID) : base(data.Position + offset) {
            // fix Theo's "crash when leaving behind and going up" bug
            Tag = 0;

            color = data.Enum("color", CrystalColor.Blue);
            this.entityID = entityID;

            floating = data.Bool("startFloating");

            // replace the sprite
            Remove(Get<Sprite>());
            Sprite replacementSprite = new Sprite(GFX.Game, data.Attr("spritePath"));
            replacementSprite.Add("sprite", "");
            replacementSprite.Play("sprite");
            replacementSprite.CenterOrigin();
            replacementSprite.Position.Y = -10;
            Add(replacementSprite);
            new DynData<TheoCrystal>(this)["sprite"] = replacementSprite;

            Add(new TransitionListener() {
                OnOutBegin = onTransitionOut
            });
        }

        private void onTransitionOut() {
            // wipe the spinner connections so that they are computed again after the transition (in the new room).
            spinnerNeighbors = null;
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            Player player = scene.Tracker.GetEntity<Player>();
            if (player?.Holding?.Entity is SpinnerBreakingBall ball && ball.entityID.Level == entityID.Level && ball.entityID.ID == entityID.ID) {
                // oops, the player is carrying a copy of ourselves from another room! commit remove self.
                RemoveSelf();
            }
        }

        public override void Update() {
            if (Hold.IsHeld) {
                floating = false;
            }
            if (floating) {
                new DynData<Holdable>(Hold)["gravityTimer"] = 1f;
            }

            base.Update();

            if (spinnerNeighbors == null) {
                // connections weren't computed yet, do that right now!
                computeSpinnerConnections();
            } else {
                // we want to check all spinners explicitly instead of just going CollideCheck<CrystalStaticSpinner>(),
                // to include those turned off because the player is too far.
                foreach (CrystalStaticSpinner candidate in spinnerNeighbors.Keys) {
                    if (candidate.CollideCheck(this)) {
                        // BOOM! recursively shatter spinners.
                        shatterSpinner(candidate);
                        RemoveSelf();
                    }
                }
            }
        }

        private void computeSpinnerConnections() {
            spinnerNeighbors = new Dictionary<CrystalStaticSpinner, HashSet<CrystalStaticSpinner>>();

            // take all spinners on screen, filter those with a matching color
            List<CrystalStaticSpinner> allSpinnersInScreen = Scene.Tracker.GetEntities<CrystalStaticSpinner>()
                .OfType<CrystalStaticSpinner>().Where(spinner => new DynData<CrystalStaticSpinner>(spinner).Get<CrystalColor>("color") == color).ToList();

            foreach (CrystalStaticSpinner spinner1 in allSpinnersInScreen) {
                if (!spinnerNeighbors.ContainsKey(spinner1)) {
                    spinnerNeighbors[spinner1] = new HashSet<CrystalStaticSpinner>();
                }

                foreach (CrystalStaticSpinner spinner2 in allSpinnersInScreen) {
                    // to connect spinners, we are using the same criteria as "spinner juice" generation in the game.
                    if (new DynData<CrystalStaticSpinner>(spinner2).Get<int>("ID") > new DynData<CrystalStaticSpinner>(spinner1).Get<int>("ID")
                        && spinner2.AttachToSolid == spinner1.AttachToSolid && (spinner2.Position - spinner1.Position).LengthSquared() < 576f) {

                        // register 2 as a neighbor of 1, and 1 as a neighbor of 2.
                        if (!spinnerNeighbors.ContainsKey(spinner2)) {
                            spinnerNeighbors[spinner2] = new HashSet<CrystalStaticSpinner>();
                        }

                        spinnerNeighbors[spinner1].Add(spinner2);
                        spinnerNeighbors[spinner2].Add(spinner1);
                    }
                }
            }
        }

        private void shatterSpinner(CrystalStaticSpinner spinner) {
            spinner.Destroy();
            shatteredSpinners.Add(spinner);

            if (spinnerNeighbors.ContainsKey(spinner)) {
                foreach (CrystalStaticSpinner neighbor in spinnerNeighbors[spinner]) {
                    if (!shatteredSpinners.Contains(neighbor)) {
                        shatterSpinner(neighbor);
                    }
                }
            }
        }

        private static void onTheoCrystalDie(On.Celeste.TheoCrystal.orig_Die orig, TheoCrystal self) {
            if (self is SpinnerBreakingBall) {
                self.RemoveSelf();
            } else {
                orig(self);
            }
        }
    }
}
