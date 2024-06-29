using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /**
     * Common behavior across all spinner breaking balls: grouping spinners, and well... breaking them.
     */
    public abstract class SpinnerBreakingBallGeneric<SpinnerType, ColorType> : TheoCrystal where SpinnerType : Entity {
        private static readonly FieldInfo spinnerIDField = typeof(SpinnerType).GetField("ID", BindingFlags.NonPublic | BindingFlags.Instance);

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

        private Task computeSpinnerNeighbors;
        private CancellationTokenSource computeSpinnerNeighborsToken;

        private Dictionary<SpinnerType, HashSet<SpinnerType>> spinnerNeighbors;
        private List<SpinnerType> listOfSpinners;
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
            listOfSpinners = null;
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

            if (spinnerNeighbors == null && computeSpinnerNeighbors == null) {
                computeSpinnerNeighborsToken = new CancellationTokenSource();
                computeSpinnerNeighbors = computeSpinnerConnections(computeSpinnerNeighborsToken.Token);
            }
            if (listOfSpinners == null) {
                listOfSpinners = Scene.Tracker.GetEntities<SpinnerType>()
                    .OfType<SpinnerType>()
                    .Where(spinner => getColor(spinner).Equals(color))
                    .ToList();
            }

            // we want to check all spinners explicitly instead of just going CollideCheck<SpinnerType>(),
            // to include those turned off because the player is too far.
            foreach (SpinnerType candidate in listOfSpinners) {
                if (candidate.Scene != null && candidate.CollideCheck(this)) {
                    if (!computeSpinnerNeighbors.IsCompleted) computeSpinnerNeighbors.Wait();
                    if (computeSpinnerNeighbors.IsFaulted) Logger.Log("MaxHelpingHand/SpinnerBreakingBall", $"Failed to compute Spinner Neighbors: {computeSpinnerNeighbors.Exception}");
                    if (spinnerNeighbors == null) return;
                    // BOOM! recursively shatter spinners.
                    shatterSpinner(candidate);
                    RemoveSelf();
                }
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            computeSpinnerNeighborsToken.Cancel();
        }
        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);
            computeSpinnerNeighborsToken.Cancel();
        }

        private Task computeSpinnerConnections(CancellationToken cancelToken) {
            return Task.Run(() => {
                Dictionary<Entity, int> idCache = new Dictionary<Entity, int>();
                Dictionary<SpinnerType, HashSet<SpinnerType>> neighbors = new Dictionary<SpinnerType, HashSet<SpinnerType>>();

                // take all spinners on screen, filter those with a matching color
                List<SpinnerType> allSpinnersInScreen = Scene.Tracker.GetEntities<SpinnerType>()
                    .OfType<SpinnerType>().Where(spinner => getColor(spinner).Equals(color)).ToList();

                foreach (SpinnerType spinner1 in allSpinnersInScreen) {
                    if (!neighbors.ContainsKey(spinner1)) {
                        neighbors[spinner1] = new HashSet<SpinnerType>();
                    }

                    foreach (SpinnerType spinner2 in allSpinnersInScreen) {
                        if (!idCache.ContainsKey(spinner1)) {
                            idCache[spinner1] = (int) spinnerIDField.GetValue(spinner1);
                        }
                        if (!idCache.ContainsKey(spinner2)) {
                            idCache[spinner2] = (int) spinnerIDField.GetValue(spinner2);
                        }

                        // to connect spinners, we are using the same criteria as "spinner juice" generation in the game.
                        if (idCache[spinner2] > idCache[spinner1]
                            && getAttachToSolid(spinner2) == getAttachToSolid(spinner1) && (spinner2.Position - spinner1.Position).LengthSquared() < 576f) {

                            // register 2 as a neighbor of 1, and 1 as a neighbor of 2.
                            if (!neighbors.ContainsKey(spinner2)) {
                                neighbors[spinner2] = new HashSet<SpinnerType>();
                            }

                            neighbors[spinner1].Add(spinner2);
                            neighbors[spinner2].Add(spinner1);
                        }
                    }
                }

                spinnerNeighbors = neighbors;
            }, cancelToken);
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
