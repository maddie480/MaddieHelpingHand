using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /**
     * Common behavior across all spinner breaking balls: grouping spinners, and well... breaking them.
     */
    public abstract class SpinnerBreakingBallGeneric<SpinnerType, ColorType> : TheoCrystal where SpinnerType : Entity {
        public static void Load() {
            On.Celeste.TheoCrystal.Die += onTheoCrystalDie;
        }

        public static void Unload() {
            On.Celeste.TheoCrystal.Die -= onTheoCrystalDie;
        }

        protected readonly ColorType color;
        private readonly EntityID entityID;

        protected Sprite sprite;

        private bool floating;

        private Dictionary<SpinnerType, HashSet<SpinnerType>> spinnerNeighbors;
        private HashSet<SpinnerType> shatteredSpinners = new HashSet<SpinnerType>();

        public SpinnerBreakingBallGeneric(EntityData data, Vector2 offset, EntityID entityID, ColorType color) : base(data.Position + offset) {
            // fix Theo's "crash when leaving behind and going up" bug
            Tag = 0;

            this.color = color;
            this.entityID = entityID;

            floating = data.Bool("startFloating");

            // replace the sprite
            Remove(Get<Sprite>());
            sprite = new Sprite(GFX.Game, data.Attr("spritePath"));
            sprite.Add("sprite", "");
            sprite.Play("sprite");
            sprite.CenterOrigin();
            sprite.Position.Y = -10;
            Add(sprite);
            new DynData<TheoCrystal>(this)["sprite"] = sprite;

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
            if (player?.Holding?.Entity is SpinnerBreakingBallGeneric<SpinnerType, ColorType> ball && ball.entityID.Level == entityID.Level && ball.entityID.ID == entityID.ID) {
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
                // we want to check all spinners explicitly instead of just going CollideCheck<SpinnerType>(),
                // to include those turned off because the player is too far.
                foreach (SpinnerType candidate in spinnerNeighbors.Keys) {
                    if (candidate.Scene != null && candidate.CollideCheck(this)) {
                        // BOOM! recursively shatter spinners.
                        shatterSpinner(candidate);
                        RemoveSelf();
                    }
                }
            }
        }

        private void computeSpinnerConnections() {
            spinnerNeighbors = new Dictionary<SpinnerType, HashSet<SpinnerType>>();

            // take all spinners on screen, filter those with a matching color
            List<SpinnerType> allSpinnersInScreen = Scene.Tracker.GetEntities<SpinnerType>()
                .OfType<SpinnerType>().Where(spinner => getColor(spinner).Equals(color)).ToList();

            foreach (SpinnerType spinner1 in allSpinnersInScreen) {
                if (!spinnerNeighbors.ContainsKey(spinner1)) {
                    spinnerNeighbors[spinner1] = new HashSet<SpinnerType>();
                }

                foreach (SpinnerType spinner2 in allSpinnersInScreen) {
                    // to connect spinners, we are using the same criteria as "spinner juice" generation in the game.
                    if (new DynData<SpinnerType>(spinner2).Get<int>("ID") > new DynData<SpinnerType>(spinner1).Get<int>("ID")
                        && getAttachToSolid(spinner2) == getAttachToSolid(spinner1) && (spinner2.Position - spinner1.Position).LengthSquared() < 576f) {

                        // register 2 as a neighbor of 1, and 1 as a neighbor of 2.
                        if (!spinnerNeighbors.ContainsKey(spinner2)) {
                            spinnerNeighbors[spinner2] = new HashSet<SpinnerType>();
                        }

                        spinnerNeighbors[spinner1].Add(spinner2);
                        spinnerNeighbors[spinner2].Add(spinner1);
                    }
                }
            }
        }

        private void shatterSpinner(SpinnerType spinner) {
            // don't break already broken spinners!
            if (spinner.Scene == null) return;

            destroySpinner(spinner);
            shatteredSpinners.Add(spinner);

            if (spinnerNeighbors.ContainsKey(spinner)) {
                foreach (SpinnerType neighbor in spinnerNeighbors[spinner]) {
                    if (!shatteredSpinners.Contains(neighbor)) {
                        shatterSpinner(neighbor);
                    }
                }
            }
        }

        private static void onTheoCrystalDie(On.Celeste.TheoCrystal.orig_Die orig, TheoCrystal self) {
            if (self is SpinnerBreakingBallGeneric<SpinnerType, ColorType>) {
                self.RemoveSelf();
            } else {
                orig(self);
            }
        }

        protected abstract ColorType getColor(SpinnerType spinner);
        protected abstract bool getAttachToSolid(SpinnerType spinner);
        protected abstract void destroySpinner(SpinnerType spinner);
    }
}
