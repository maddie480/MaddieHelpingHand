using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // This is Sideways Jumpthrus, except attached to a solid, using similar magic to Sideways Moving Platforms to handle player collision.
    [CustomEntity("MaxHelpingHand/AttachedSidewaysJumpThru")]
    [Tracked]
    [TrackedAs(typeof(SidewaysJumpThru))]
    public class AttachedSidewaysJumpThru : SidewaysJumpThru {
        // solid used internally to push/squash/carry the player around
        private Solid playerInteractingSolid;

        public readonly new bool Left;

        public DashCollision OnDashCollide;
        private Vector2 shakeOffset = Vector2.Zero;

        public AttachedSidewaysJumpThru(EntityData data, Vector2 offset) : base(data, offset) {
            Left = data.Bool("left");

            // this solid will be made solid only when moving the player with the platform, so that the player gets squished and can climb the platform properly.
            playerInteractingSolid = new Solid(Position, Width, Height, safe: false);
            playerInteractingSolid.Collidable = false;
            playerInteractingSolid.Visible = false;
            if (!Left) {
                playerInteractingSolid.Position.X += 3f;
            }

            // create the StaticMover that will make this jumpthru attached.
            StaticMover staticMover = new StaticMoverWithLiftSpeed() {
                SolidChecker = solid => solid.CollideRect(new Rectangle((int) X, (int) Y - 1, (int) Width, (int) Height + 2)),
                OnMove = move => SidewaysMovingPlatform.SidewaysJumpthruOnMove(this, playerInteractingSolid, Left, move),
                OnShake = onShake,
                OnSetLiftSpeed = liftSpeed => playerInteractingSolid.LiftSpeed = liftSpeed
            };
            Add(staticMover);
        }

        private void onShake(Vector2 move) {
            shakeOffset += move;
            playerInteractingSolid.ShakeStaticMovers(move);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // add the hidden solid to the scene as well.
            scene.Add(playerInteractingSolid);
        }

        public override void Render() {
            Position += shakeOffset;
            base.Render();
            Position -= shakeOffset;
        }
    }
}
