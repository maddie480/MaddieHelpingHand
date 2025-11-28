using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
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
        private static List<IDisposable> allSetHooks = new List<IDisposable>();

        private static bool frostHelperHooked = false;
        private static bool cherryHelperHooked = false;
        private static bool sardine7Hooked = false;

        private static bool kevinBarriersAreCollidable = false;

        public static void Load() {
            On.Celeste.LevelLoader.LoadingThread += onLevelLoadingThread;
            On.Celeste.Actor.MoveHExact += onActorMoveHExact;
            On.Celeste.Actor.MoveVExact += onActorMoveVExact;

            hookKevin(typeof(CrushBlock));
        }

        public static void HookMods() {
            if (!frostHelperHooked) {
                Type slowKevinClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "FrostHelper.FrostModule")?.GetType().Assembly
                    .GetType("FrostHelper.CustomCrushBlock");
                if (slowKevinClass != null) {
                    hookKevin(slowKevinClass);
                    frostHelperHooked = true;
                }
            }

            if (!cherryHelperHooked) {
                Type nonReturnKevinClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "Celeste.Mod.CherryHelper.CherryHelper")?.GetType().Assembly
                    .GetType("Celeste.Mod.CherryHelper.NonReturnCrushBlock");
                if (nonReturnKevinClass != null) {
                    hookKevin(nonReturnKevinClass);
                    cherryHelperHooked = true;
                }

                Type uninterruptedNonReturnKevinClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "Celeste.Mod.CherryHelper.CherryHelper")?.GetType().Assembly
                    .GetType("Celeste.Mod.CherryHelper.UninterruptedNRCB");
                if (uninterruptedNonReturnKevinClass != null) {
                    hookKevin(uninterruptedNonReturnKevinClass);
                    cherryHelperHooked = true;
                }

                Type nonReturnSokobanClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "Celeste.Mod.CherryHelper.CherryHelper")?.GetType().Assembly
                    .GetType("Celeste.Mod.CherryHelper.NonReturnSokoban");
                if (nonReturnSokobanClass != null) {
                    hookKevin(nonReturnSokobanClass);
                    cherryHelperHooked = true;
                }
            }

            if (!sardine7Hooked) {
                Type sokobanBlockClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "Celeste.Mod.Sardine7.Sardine7Module")?.GetType().Assembly
                    .GetType("Celeste.Mod.Sardine7.Entities.SokobanBlock");
                if (sokobanBlockClass != null) {
                    hookKevin(sokobanBlockClass);
                    sardine7Hooked = true;
                }
            }
        }

        // everyone is copy pasting vanilla Kevin, so everyone has the same methods, that's handy :p
        private static void hookKevin(Type type) {
            MethodInfo onMoveHCheck = typeof(KevinBarrier).GetMethod("onKevinMoveHCheck", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo onMoveVCheck = typeof(KevinBarrier).GetMethod("onKevinMoveVCheck", BindingFlags.NonPublic | BindingFlags.Static);

            allSetHooks.Add(new Hook(type.GetMethod("MoveHCheck", BindingFlags.NonPublic | BindingFlags.Instance), onMoveHCheck));
            allSetHooks.Add(new Hook(type.GetMethod("MoveVCheck", BindingFlags.NonPublic | BindingFlags.Instance), onMoveVCheck));
            allSetHooks.Add(new ILHook(type.GetMethod("MoveHCheck", BindingFlags.NonPublic | BindingFlags.Instance), modKevinMoveCheck));
            allSetHooks.Add(new ILHook(type.GetMethod("MoveVCheck", BindingFlags.NonPublic | BindingFlags.Instance), modKevinMoveCheck));
        }

        public static void Unload() {
            On.Celeste.LevelLoader.LoadingThread -= onLevelLoadingThread;
            On.Celeste.Actor.MoveHExact -= onActorMoveHExact;
            On.Celeste.Actor.MoveVExact -= onActorMoveVExact;

            foreach (IDisposable h in allSetHooks) {
                h.Dispose();
            }
            allSetHooks.Clear();

            frostHelperHooked = false;
            cherryHelperHooked = false;
            sardine7Hooked = false;
        }

        private static void onLevelLoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            // spawn a Kevin barrier renderer if there are Kevin barriers in the map.
            if (new DynData<LevelLoader>(self).Get<Session>("session").MapData?.Levels?.Any(level => level.Entities?.Any(entity => EntityNameRegistry.KevinBarriers.Contains(entity.Name)) ?? false) ?? false) {
                self.Level.Add(new KevinBarrierRenderer());
            }

            orig(self);
        }

        private static bool onKevinMoveHCheck(Func<Solid, float, bool> orig, Solid self, float amount) {
            kevinBarriersAreCollidable = true;
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

            kevinBarriersAreCollidable = false;
            foreach (KevinBarrier barrier in kevinBarriers) {
                barrier.Collidable = false;
            }
            return isHit;
        }

        private static bool onKevinMoveVCheck(Func<Solid, float, bool> orig, Solid self, float amount) {
            kevinBarriersAreCollidable = true;
            IEnumerable<KevinBarrier> kevinBarriers = self.Scene.Tracker.GetEntities<KevinBarrier>().OfType<KevinBarrier>();
            foreach (KevinBarrier barrier in kevinBarriers) {
                barrier.Collidable = true;
            }

            bool isHit = orig(self, amount);

            kevinBarriersAreCollidable = false;
            foreach (KevinBarrier barrier in kevinBarriers) {
                barrier.Collidable = false;
            }
            return isHit;
        }

        private static void modKevinMoveCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(instr => instr.MatchCall<Platform>("MoveVCollideSolidsAndBounds") || instr.MatchCall<Platform>("MoveHCollideSolidsAndBounds"))
                && cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdnull())) {

                Logger.Log("MaxHelpingHand/KevinBarrier", $"Modding Kevin collide check to add barrier hit at {cursor.Index} in IL for {il.Method.FullName}");

                // replace onCollide (which is of type Action<Vector2, Vector2, Platform>) to notify Kevin barriers that are being hit.
                cursor.EmitDelegate<Func<Action<Vector2, Vector2, Platform>, Action<Vector2, Vector2, Platform>>>(notifyKevinBarriers);
            }
        }
        private static Action<Vector2, Vector2, Platform> notifyKevinBarriers(Action<Vector2, Vector2, Platform> orig) {
            return (a, b, collidedPlatform) => {
                if (collidedPlatform is KevinBarrier barrier) {
                    barrier.hitByKevin();
                } else {
                    orig(a, b, collidedPlatform);
                }
            };
        }

        private static bool onActorMoveHExact(On.Celeste.Actor.orig_MoveHExact orig, Actor self, int moveH, Collision onCollide, Solid pusher) {
            if (!kevinBarriersAreCollidable) {
                return orig(self, moveH, onCollide, pusher);
            }

            // make sure the barriers are not collidable while actors are being moved.
            return turnOffBarrierCollisionThenRun(self, () => orig(self, moveH, onCollide, pusher));
        }

        private static bool onActorMoveVExact(On.Celeste.Actor.orig_MoveVExact orig, Actor self, int moveV, Collision onCollide, Solid pusher) {
            if (!kevinBarriersAreCollidable) {
                return orig(self, moveV, onCollide, pusher);
            }

            // make sure the barriers are not collidable while actors are being moved.
            return turnOffBarrierCollisionThenRun(self, () => orig(self, moveV, onCollide, pusher));
        }

        private static bool turnOffBarrierCollisionThenRun(Actor self, Func<bool> function) {
            IEnumerable<KevinBarrier> kevinBarriers = self.Scene.Tracker.GetEntities<KevinBarrier>().OfType<KevinBarrier>();
            foreach (KevinBarrier barrier in kevinBarriers) {
                barrier.Collidable = false;
            }

            bool result = function();

            foreach (KevinBarrier barrier in kevinBarriers) {
                barrier.Collidable = true;
            }

            return result;
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
