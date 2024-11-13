using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    // not really a trigger, but that works as well.
    [CustomEntity("MaxHelpingHand/CameraOffsetBorder")]
    [Tracked]
    public class CameraOffsetBorder : Trigger {
        private readonly bool topLeft, topCenter, topRight, centerLeft, centerRight, bottomLeft, bottomCenter, bottomRight, inside, inverted;
        private readonly string flag;

        public CameraOffsetBorder(EntityData data, Vector2 offset) : base(data, offset) {
            topLeft = data.Bool("topLeft");
            topCenter = data.Bool("topCenter");
            topRight = data.Bool("topRight");
            centerLeft = data.Bool("centerLeft");
            centerRight = data.Bool("centerRight");
            bottomLeft = data.Bool("bottomLeft");
            bottomCenter = data.Bool("bottomCenter");
            bottomRight = data.Bool("bottomRight");
            inside = data.Bool("inside");

            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            AddTag(Tags.TransitionUpdate);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Update();
        }

        public override void Update() {
            base.Update();

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                if (!string.IsNullOrEmpty(flag) && !(Scene as Level).Session.GetFlag(flag) && !inverted) {
                    // trigger is flag-toggled, but the associated flag is disabled and the trigger has "inverted" disabled.
                    Collidable = false;
                } else if (!string.IsNullOrEmpty(flag) && (Scene as Level).Session.GetFlag(flag) && inverted) {
                    // trigger is flag-toggled, but the associated flag is enabled and the trigger has "inverted" enabled.
                    Collidable = false;
                } else {
                    bool top = player.Bottom <= Top;
                    bool centerY = player.Bottom > Top && player.Top < Bottom;
                    bool bottom = player.Top >= Bottom;
                    bool left = player.Right <= Left;
                    bool centerX = player.Right > Left && player.Left < Right;
                    bool right = player.Left >= Right;

                    Collidable =
                        (topLeft && top && left) ||
                        (topCenter && top && centerX) ||
                        (topRight && top && right) ||
                        (centerLeft && centerY && left) ||
                        (centerRight && centerY && right) ||
                        (bottomLeft && bottom && left) ||
                        (bottomCenter && bottom && centerX) ||
                        (bottomRight && bottom && right) ||
                        (inside && centerX && centerY);
                }
            }
        }

        private static Hook hookPlayerCameraTarget;

        public static void Load() {
            hookPlayerCameraTarget = new Hook(
                typeof(Player).GetMethod("get_CameraTarget"),
                typeof(CameraOffsetBorder).GetMethod("modCameraTarget", BindingFlags.NonPublic | BindingFlags.Static));
        }

        public static void Unload() {
            hookPlayerCameraTarget?.Dispose();
            hookPlayerCameraTarget = null;
        }


        private static Vector2 modCameraTarget(Func<Player, Vector2> orig, Player self) {
            if (self.Scene == null) return orig(self);

            Vector2 target = orig(self);

            Rectangle viewpoint = new Rectangle((int) target.X, (int) target.Y, MaxHelpingHandModule.CameraWidth, MaxHelpingHandModule.CameraHeight);
            foreach (CameraOffsetBorder border in self.Scene.Tracker.GetEntities<CameraOffsetBorder>()) {
                while (border.Collidable && border.CollideRect(viewpoint)) {
                    // the border is enabled and on-screen, unacceptable!
                    if (self.Left <= border.Right && viewpoint.Right >= border.Left && (border.topLeft || border.centerLeft || border.bottomLeft)) {
                        // player is on the left, camera is too far right => push camera to the left.
                        target.X--;
                        viewpoint.X--;
                    } else if (self.Right >= border.Left && viewpoint.Left <= border.Right && (border.topRight || border.centerRight || border.bottomRight)) {
                        // player is on the right, camera is too far left => push camera to the right.
                        target.X++;
                        viewpoint.X++;
                    } else if (self.Bottom >= border.Top && viewpoint.Top <= border.Bottom && (border.bottomLeft || border.bottomCenter || border.bottomRight)) {
                        // player is on the bottom, camera is too far up => push camera to the bottom.
                        target.Y++;
                        viewpoint.Y++;
                    } else if (self.Top <= border.Bottom && viewpoint.Bottom >= border.Top && (border.topLeft || border.topCenter || border.topRight)) {
                        // player is on the top, camera is too far down => push camera to the top;
                        target.Y--;
                        viewpoint.Y--;
                    } else {
                        Logger.Log("MaxHelpingHand/CameraOffsetBorder", "Camera offset border is on-screen but we didn't find any way to prevent that from happening!");
                        break;
                    }
                }
            }

            return target;
        }
    }
}
