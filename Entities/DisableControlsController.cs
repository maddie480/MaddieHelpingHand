using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // cursed controller that disable arbitrary player controls, used in a less customizable way in Flusheline Collab.
    [CustomEntity("MaxHelpingHand/DisableControlsController")]
    [Tracked]
    public class DisableControlsController : Entity {
        private static Hook hookButtonCheck;
        private static Hook hookButtonPressed;
        private static Hook hookButtonReleased;
        private static Hook hookGrabCheck;
        private static Hook hookDashPressed;
        private static Hook hookCrouchDashPressed;

        public static void Load() {
            // break directions
            On.Celeste.Player.Update += breakTheControls;

            // break Input.X.Check, Input.X.Pressed, Input.X.Released with X being Jump, Dash, Grab or CrouchDash
            hookButtonCheck = new Hook(typeof(VirtualButton).GetMethod("get_Check"), typeof(DisableControlsController).GetMethod("hookOnButton", BindingFlags.NonPublic | BindingFlags.Static));
            hookButtonPressed = new Hook(typeof(VirtualButton).GetMethod("get_Pressed"), typeof(DisableControlsController).GetMethod("hookOnButton", BindingFlags.NonPublic | BindingFlags.Static));
            hookButtonReleased = new Hook(typeof(VirtualButton).GetMethod("get_Released"), typeof(DisableControlsController).GetMethod("hookOnButton", BindingFlags.NonPublic | BindingFlags.Static));

            // break Input.GrabCheck and Input.DashPressed
            hookGrabCheck = new Hook(typeof(Input).GetMethod("get_GrabCheck"), typeof(DisableControlsController).GetMethod("modGrabResult", BindingFlags.NonPublic | BindingFlags.Static));
            hookDashPressed = new Hook(typeof(Input).GetMethod("get_DashPressed"), typeof(DisableControlsController).GetMethod("modDashResult", BindingFlags.NonPublic | BindingFlags.Static));
            hookCrouchDashPressed = new Hook(typeof(Input).GetMethod("get_CrouchDashPressed"), typeof(DisableControlsController).GetMethod("modDashResult", BindingFlags.NonPublic | BindingFlags.Static));
        }

        public static void Unload() {
            On.Celeste.Player.Update -= breakTheControls;

            hookButtonCheck?.Dispose();
            hookButtonPressed?.Dispose();
            hookButtonReleased?.Dispose();
            hookGrabCheck?.Dispose();
            hookDashPressed?.Dispose();
            hookCrouchDashPressed?.Dispose();

            hookButtonCheck = null;
            hookButtonPressed = null;
            hookButtonReleased = null;
            hookGrabCheck = null;
            hookDashPressed = null;
            hookCrouchDashPressed = null;
        }

        private readonly bool up, down, left, right, jump, grab, dash;

        public DisableControlsController(EntityData data, Vector2 offset)
            : base(data.Position + offset) {

            up = data.Bool("up");
            down = data.Bool("down");
            left = data.Bool("left");
            right = data.Bool("right");
            jump = data.Bool("jump");
            grab = data.Bool("grab");
            dash = data.Bool("dash");
        }

        private static void breakTheControls(On.Celeste.Player.orig_Update orig, Player self) {
            DisableControlsController c = getDisableControlsControllerInRoomSafely();
            if (c == null) {
                // we don't want to disable controls here!
                orig(self);
                return;
            }

            Vector2 oldAim = Input.Aim;
            int oldMoveX = Input.MoveX.Value;
            int oldMoveY = Input.MoveY.Value;

            Vector2 newAim = Input.Aim;
            int newMoveX = Input.MoveX.Value;
            int newMoveY = Input.MoveY.Value;

            if (c.up) {
                // Y cannot be negative
                newAim.Y = Math.Max(0, newAim.Y);
                newMoveY = Math.Max(0, newMoveY);
            }
            if (c.down) {
                // Y cannot be positive
                newAim.Y = Math.Min(0, newAim.Y);
                newMoveY = Math.Min(0, newMoveY);
            }
            if (c.left) {
                // X cannot be negative
                newAim.X = Math.Max(0, newAim.X);
                newMoveX = Math.Max(0, newMoveX);
            }
            if (c.right) {
                // X cannot be positive
                newAim.X = Math.Min(0, newAim.X);
                newMoveX = Math.Min(0, newMoveX);
            }

            new DynData<VirtualJoystick>(Input.Aim)["Value"] = newAim;
            Input.MoveX.Value = newMoveX;
            Input.MoveY.Value = newMoveY;

            orig(self);

            new DynData<VirtualJoystick>(Input.Aim)["Value"] = oldAim;
            Input.MoveX.Value = oldMoveX;
            Input.MoveY.Value = oldMoveY;
        }

        private static bool hookOnButton(Func<VirtualButton, bool> orig, VirtualButton self) {
            DisableControlsController c = getDisableControlsControllerInRoomSafely();
            if (c == null) {
                // we don't want to disable controls here!
                return orig(self);
            }

            if (((self == Input.Dash || self == Input.CrouchDash) && c.dash)
                || (self == Input.Jump && c.jump)
                || (self == Input.Grab && c.grab)) {

                return false;
            }

            return orig(self);
        }

        private static bool modGrabResult(Func<bool> orig) {
            DisableControlsController c = getDisableControlsControllerInRoomSafely();
            if (c == null || !c.grab) {
                // we don't want to disable grab
                return orig();
            }

            return false;
        }

        private static bool modDashResult(Func<bool> orig) {
            DisableControlsController c = getDisableControlsControllerInRoomSafely();
            if (c == null || !c.dash) {
                // we don't want to disable dash
                return orig();
            }

            return false;
        }

        private static DisableControlsController getDisableControlsControllerInRoomSafely() {
            // return null if the DisableControlsController type isn't tracked yet. This can happen when the mod is being loaded during runtime
            if (!Engine.Scene.Tracker.Entities.ContainsKey(typeof(DisableControlsController))) return null;

            return Engine.Scene.Tracker.GetEntity<DisableControlsController>();
        }
    }
}
