using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/NoDashRefillSpring", "MaxHelpingHand/NoDashRefillSpringLeft", "MaxHelpingHand/NoDashRefillSpringRight")]
    class NoDashRefillSpring : Spring {
        private static MethodInfo bounceAnimate = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.NonPublic | BindingFlags.Instance);
        private static object[] noParams = new object[0];

        public NoDashRefillSpring(EntityData data, Vector2 offset)
            : base(data.Position + offset, GetOrientationFromName(data.Name), data.Bool("playerCanUse", true)) {

            DynData<Spring> selfSpring = new DynData<Spring>(this);

            // remove the vanilla player collider. this is the one thing we want to mod here.
            foreach (Component component in this) {
                if (component.GetType() == typeof(PlayerCollider)) {
                    Remove(component);
                    break;
                }
            }

            // replace it with our own collider.
            if (data.Bool("playerCanUse", true)) {
                Add(new PlayerCollider(OnCollide));
            }

            // replace the vanilla sprite with our custom one.
            Sprite sprite = selfSpring.Get<Sprite>("sprite");
            sprite.Reset(GFX.Game, data.Attr("spriteDirectory", "objects/MaxHelpingHand/noDashRefillSpring") + "/");
            sprite.Add("idle", "", 0f, default(int));
            sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
            sprite.Add("disabled", "white", 0.07f);
            sprite.Play("idle");
            sprite.Origin.X = sprite.Width / 2f;
            sprite.Origin.Y = sprite.Height;
        }

        private static Orientations GetOrientationFromName(string name) {
            switch (name) {
                case "MaxHelpingHand/NoDashRefillSpring":
                    return Orientations.Floor;
                case "MaxHelpingHand/NoDashRefillSpringRight":
                    return Orientations.WallRight;
                case "MaxHelpingHand/NoDashRefillSpringLeft":
                    return Orientations.WallLeft;
                default:
                    throw new Exception("No Dash Refill Spring name doesn't correlate to a valid Orientation!");
            }
        }


        private void OnCollide(Player player) {
            if (player.StateMachine.State == 9) {
                return;
            }

            // Save dash count. Dashes are reloaded by SideBounce and SuperBounce.
            int originalDashCount = player.Dashes;

            if (Orientation == Orientations.Floor) {
                if (player.Speed.Y >= 0f) {
                    bounceAnimate.Invoke(this, noParams);
                    player.SuperBounce(Top);
                }
            } else if (Orientation == Orientations.WallLeft) {
                if (player.SideBounce(1, Right, CenterY)) {
                    bounceAnimate.Invoke(this, noParams);
                }
            } else if (Orientation == Orientations.WallRight) {
                if (player.SideBounce(-1, Left, CenterY)) {
                    bounceAnimate.Invoke(this, noParams);
                }
            } else {
                throw new Exception("Orientation not supported!");
            }

            // Restore original dash count.
            player.Dashes = originalDashCount;
        }
    }
}
