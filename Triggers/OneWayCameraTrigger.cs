using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/OneWayCameraTrigger")]
    [Tracked]
    public class OneWayCameraTrigger : Trigger {
        private readonly bool left, right, up, down;
        private readonly bool blockPlayer;
        private readonly string flag;

        private float minX = float.MinValue, maxX = float.MaxValue, minY = float.MinValue, maxY = float.MaxValue;

        private InvisibleBarrier upperBound, leftBound, rightBound;
        private Killbox lowerBound;

        public OneWayCameraTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            // parse options
            left = data.Bool("left");
            right = data.Bool("right");
            up = data.Bool("up");
            down = data.Bool("down");
            blockPlayer = data.Bool("blockPlayer");
            flag = data.Attr("flag");
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Update();
        }

        public override void Update() {
            base.Update();

            // toggle the trigger according to the flag
            if (!string.IsNullOrEmpty(flag)) {
                Collidable = SceneAs<Level>().Session.GetFlag(flag);
            }
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            // create boundaries if enabled.
            if (blockPlayer) {
                Level level = SceneAs<Level>();
                if (!left) {
                    Scene.Add(leftBound = new InvisibleBarrier(level.Camera.Position - Vector2.UnitX * 9, 8, MaxHelpingHandModule.GameplayHeight));
                }
                if (!right) {
                    Scene.Add(rightBound = new InvisibleBarrier(level.Camera.Position + Vector2.UnitX * MaxHelpingHandModule.GameplayWidth, 8, MaxHelpingHandModule.GameplayHeight));
                }
                if (!up) {
                    Scene.Add(upperBound = new InvisibleBarrier(level.Camera.Position - Vector2.UnitY * 9, MaxHelpingHandModule.GameplayWidth, 8));
                }
                if (!down) {
                    Scene.Add(lowerBound = new Killbox(new EntityData { Width = MaxHelpingHandModule.GameplayWidth }, level.Camera.Position + Vector2.UnitY * (MaxHelpingHandModule.GameplayHeight + 6)));
                }
            }
        }

        public override void OnStay(Player player) {
            base.OnStay(player);

            // cap the left/right/top/bottom position of the camera to the current values, depending on the set options.
            Level level = SceneAs<Level>();
            if (!left) {
                minX = Math.Max(minX, level.Camera.X);
            }
            if (!right) {
                maxX = Math.Min(maxX, level.Camera.X);
            }
            if (!up) {
                minY = Math.Max(minY, level.Camera.Y);
            }
            if (!down) {
                maxY = Math.Min(maxY, level.Camera.Y);
            }

            // update the boundaries' position, if they exist.
            if (leftBound != null) {
                leftBound.Position = level.Camera.Position - Vector2.UnitX * 9;
            }
            if (rightBound != null) {
                rightBound.Position = level.Camera.Position + Vector2.UnitX * MaxHelpingHandModule.GameplayWidth;
            }
            if (upperBound != null) {
                upperBound.Position = level.Camera.Position - Vector2.UnitY * 9;
            }
            if (lowerBound != null) {
                lowerBound.Position = level.Camera.Position + Vector2.UnitY * (MaxHelpingHandModule.GameplayHeight + 6f);
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            // reset caps.
            minX = float.MinValue;
            minY = float.MinValue;
            maxX = float.MaxValue;
            maxY = float.MaxValue;

            // remove boundary entities.
            leftBound?.RemoveSelf();
            rightBound?.RemoveSelf();
            upperBound?.RemoveSelf();
            lowerBound?.RemoveSelf();
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);

            // remove boundary entities.
            leftBound?.RemoveSelf();
            rightBound?.RemoveSelf();
            upperBound?.RemoveSelf();
            lowerBound?.RemoveSelf();
        }

        private static ILHook hookOrigUpdate;
        private static ILHook hookLoadLevel;
        private static ILHook hookTransitionRoutine;

        public static void Load() {
            hookOrigUpdate = new ILHook(typeof(Player).GetMethod("orig_Update"), ilHookCameraTarget);
            hookLoadLevel = new ILHook(typeof(Level).GetMethod("orig_LoadLevel"), ilHookGetFullCameraTarget);
            hookTransitionRoutine = new ILHook(typeof(Level).GetMethod("orig_TransitionRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), ilHookGetFullCameraTarget);
        }

        public static void Unload() {
            hookOrigUpdate?.Dispose();
            hookLoadLevel?.Dispose();
            hookTransitionRoutine?.Dispose();

            hookOrigUpdate = null;
            hookLoadLevel = null;
            hookTransitionRoutine = null;
        }

        private static void ilHookCameraTarget(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("get_CameraTarget"))) {
                Logger.Log("MaxHelpingHand/CameraOffsetBorder", $"Enforcing camera offset borders at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.EmitDelegate<Func<Vector2, Vector2>>(modCameraTarget);
            }
        }

        private static void ilHookGetFullCameraTarget(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Level>("GetFullCameraTargetAt"))) {
                Logger.Log("MaxHelpingHand/CameraOffsetBorder", $"Enforcing camera offset borders at {cursor.Index} in IL for {il.Method.FullName}");
                cursor.EmitDelegate<Func<Vector2, Vector2>>(modCameraTarget);
            }
        }

        private static Vector2 modCameraTarget(Vector2 orig) {
            Player p = Engine.Scene.Tracker.GetEntity<Player>();
            if (p == null) return orig;

            // bound the camera target to the targets allowed by the one-way camera trigger.
            foreach (OneWayCameraTrigger trigger in p.CollideAll<OneWayCameraTrigger>()) {
                orig.X = MathHelper.Clamp(orig.X, trigger.minX, trigger.maxX);
                orig.Y = MathHelper.Clamp(orig.Y, trigger.minY, trigger.maxY);
            }
            return orig;
        }
    }
}
