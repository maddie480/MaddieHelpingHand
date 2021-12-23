using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/KevinBarrier")]
    [Tracked]
    public class KevinBarrier : Solid {
        private static List<Hook> allSetHooks = new List<Hook>();

        public static void Load() {
            On.Celeste.LevelLoader.LoadingThread += onLevelLoadingThread;

            hookKevin(typeof(CrushBlock));

            Type slowKevinClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "FrostHelper.FrostModule")?.GetType().Assembly
                .GetType("FrostHelper.CustomCrushBlock");
            if (slowKevinClass != null) {
                hookKevin(slowKevinClass);
            }

            Type nonReturnKevinClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "Celeste.Mod.CherryHelper.CherryHelper")?.GetType().Assembly
                .GetType("Celeste.Mod.CherryHelper.NonReturnCrushBlock");
            if (nonReturnKevinClass != null) {
                hookKevin(nonReturnKevinClass);
            }

            Type uninterruptedNonReturnKevinClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "Celeste.Mod.CherryHelper.CherryHelper")?.GetType().Assembly
                .GetType("Celeste.Mod.CherryHelper.UninterruptedNRCB");
            if (uninterruptedNonReturnKevinClass != null) {
                hookKevin(uninterruptedNonReturnKevinClass);
            }

            Type nonReturnSokobanClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "Celeste.Mod.CherryHelper.CherryHelper")?.GetType().Assembly
                .GetType("Celeste.Mod.CherryHelper.NonReturnSokoban");
            if (nonReturnSokobanClass != null) {
                hookKevin(nonReturnSokobanClass);
            }
        }

        public static void Initialize() {
            // using Sardine7 too early makes the game crash, because particles need to be loaded in order to initialize the static fields in SokobanBlock.
            Type sokobanBlockClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "Celeste.Mod.Sardine7.Sardine7Module")?.GetType().Assembly
                .GetType("Celeste.Mod.Sardine7.Entities.SokobanBlock");
            if (sokobanBlockClass != null) {
                hookKevin(sokobanBlockClass);
            }
        }

        // everyone is copy pasting vanilla Kevin, so everyone has the same methods, that's handy :p
        private static void hookKevin(Type type) {
            MethodInfo onMoveHCheck = typeof(KevinBarrier).GetMethod("onKevinMoveHCheck", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo onMoveVCheck = typeof(KevinBarrier).GetMethod("onKevinMoveVCheck", BindingFlags.NonPublic | BindingFlags.Static);

            allSetHooks.Add(new Hook(type.GetMethod("MoveHCheck", BindingFlags.NonPublic | BindingFlags.Instance), onMoveHCheck));
            allSetHooks.Add(new Hook(type.GetMethod("MoveVCheck", BindingFlags.NonPublic | BindingFlags.Instance), onMoveVCheck));
        }

        public static void Unload() {
            On.Celeste.LevelLoader.LoadingThread -= onLevelLoadingThread;

            foreach (Hook h in allSetHooks) {
                h.Dispose();
            }
            allSetHooks.Clear();
        }

        private static void onLevelLoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            // spawn a Kevin barrier renderer if there are Kevin barriers in the map.
            if (new DynData<LevelLoader>(self).Get<Session>("session").MapData?.Levels?.Any(level => level.Entities?.Any(entity => entity.Name == "MaxHelpingHand/KevinBarrier") ?? false) ?? false) {
                self.Level.Add(new KevinBarrierRenderer());
            }

            orig(self);
        }

        private static bool onKevinMoveHCheck(Func<Solid, float, bool> orig, Solid self, float amount) {
            IEnumerable<KevinBarrier> kevinBarriers = self.Scene.Tracker.GetEntities<KevinBarrier>().OfType<KevinBarrier>();
            foreach (KevinBarrier barrier in kevinBarriers) {
                barrier.Collidable = true;
            }

            bool isHit = orig(self, amount);
            if (isHit) {
                self.MoveHCollideSolidsAndBounds(self.Scene as Level, amount, thruDashBlocks: true, (a, b, collidedPlatform) => {
                    if (collidedPlatform is KevinBarrier barrier) {
                        barrier.hitByKevin();
                    }
                });
            }

            foreach (KevinBarrier barrier in kevinBarriers) {
                barrier.Collidable = false;
            }
            return isHit;
        }

        private static bool onKevinMoveVCheck(Func<Solid, float, bool> orig, Solid self, float amount) {
            IEnumerable<KevinBarrier> kevinBarriers = self.Scene.Tracker.GetEntities<KevinBarrier>().OfType<KevinBarrier>();
            foreach (KevinBarrier barrier in kevinBarriers) {
                barrier.Collidable = true;
            }

            bool isHit = orig(self, amount);
            if (isHit) {
                self.MoveVCollideSolidsAndBounds(self.Scene as Level, amount, thruDashBlocks: true, (a, b, collidedPlatform) => {
                    if (collidedPlatform is KevinBarrier barrier) {
                        barrier.hitByKevin();
                    }
                }, checkBottom: false);
            }

            foreach (KevinBarrier barrier in kevinBarriers) {
                barrier.Collidable = false;
            }
            return isHit;
        }

        private float flash = 0f;
        private bool flashing = false;

        internal float Solidify = 0f;
        private float solidifyDelay = 0f;
        internal Color Color;
        private Color particleColor;
        private bool flashOnHit;
        internal bool Invisible;

        private List<Vector2> particles = new List<Vector2>();
        private List<KevinBarrier> adjacent = new List<KevinBarrier>();

        private float[] speeds = new float[3] { 12f, 20f, 40f };

        public KevinBarrier(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, data.Height, safe: false) {

            Collidable = false;
            for (int i = 0; i < Width * Height / 16f; i++) {
                particles.Add(new Vector2(Calc.Random.NextFloat(Width - 1f), Calc.Random.NextFloat(Height - 1f)));
            }

            Color = Calc.HexToColor(data.Attr("color", "62222b"));
            particleColor = Calc.HexToColor(data.Attr("particleColor", "ffffff"));
            flashOnHit = data.Bool("flashOnHit", true);
            Invisible = data.Bool("invisible", false);
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            scene.Tracker.GetEntity<KevinBarrierRenderer>().Track(this);
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            scene.Tracker.GetEntity<KevinBarrierRenderer>().Untrack(this);
        }

        public override void Update() {
            if (flashing) {
                flash = Calc.Approach(flash, 0f, Engine.DeltaTime * 4f);
                if (flash <= 0f) {
                    flashing = false;
                }
            } else if (solidifyDelay > 0f) {
                solidifyDelay -= Engine.DeltaTime;
            } else if (Solidify > 0f) {
                Solidify = Calc.Approach(Solidify, 0f, Engine.DeltaTime);
            }

            int speedCount = speeds.Length;
            float height = Height;
            int count = particles.Count;
            for (int i = 0; i < count; i++) {
                Vector2 particlePosition = particles[i] + Vector2.UnitY * speeds[i % speedCount] * Engine.DeltaTime;
                particlePosition.Y %= height - 1f;
                particles[i] = particlePosition;
            }
            base.Update();
        }

        private void hitByKevin() {
            flash = 1f;
            Solidify = 1f;
            solidifyDelay = 1f;
            flashing = true;
            Scene.CollideInto(new Rectangle((int) X, (int) Y - 2, (int) Width, (int) Height + 4), adjacent);
            Scene.CollideInto(new Rectangle((int) X - 2, (int) Y, (int) Width + 4, (int) Height), adjacent);
            foreach (KevinBarrier item in adjacent) {
                if (!item.flashing) {
                    item.hitByKevin();
                }
            }
            adjacent.Clear();
        }

        public override void Render() {
            if (!Invisible) {
                Color color = particleColor * 0.5f;
                foreach (Vector2 particle in particles) {
                    Draw.Pixel.Draw(Position + particle, Vector2.Zero, color);
                }
            }
            if (flashing && flashOnHit) {
                Draw.Rect(Collider, Color.White * flash * 0.5f);
            }
        }
    }
}
