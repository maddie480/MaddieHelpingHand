using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // This is an upside-down moving platform, aka a moving upside-down jumpthru.
    // As if upside-down jumpthrus didn't cause enough issues by themselves...
    [CustomEntity("MaxHelpingHand/UpsideDownMovingPlatform")]
    [TrackedAs(typeof(UpsideDownJumpThru))]
    public class UpsideDownMovingPlatform : UpsideDownJumpThru {
        // this variable is private, static, and never modified: so we only need reflection once to get it!
        private static readonly HashSet<Actor> solidRiders = (HashSet<Actor>) typeof(Solid).GetField("riders", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

        // settings
        private readonly EntityData thisEntityData;
        private readonly Vector2 thisOffset;
        private readonly string texture;

        private MTexture[] textures;

        // solid used internally to push/squash/carry the player around
        private Solid playerInteractingSolid;

        // the moving platform that makes that moving platform move :theoreticalwoke:
        private MultiNodeMovingPlatform animatingPlatform;
        private bool spawnedByOtherPlatform = false;

        public UpsideDownMovingPlatform(EntityData data, Vector2 offset) : base(data, offset) {
            // the upside-down jumpthru should be invisible.
            overrideTexture = "MaxHelpingHand/invisible";

            thisEntityData = data;
            thisOffset = offset;
            texture = data.Attr("texture", "default");

            // this solid will be made solid only when moving the player with the platform, so that the player gets squished and can climb the platform properly.
            playerInteractingSolid = new Solid(Position + Vector2.UnitY * 3f, Width, Height, safe: false);
            playerInteractingSolid.Collidable = false;
            playerInteractingSolid.Visible = false;
        }

        public override void Added(Scene scene) {
            base.Added(scene);


            // add the hidden solid to the scene as well.
            scene.Add(playerInteractingSolid);

            // load the texture.
            MTexture fullTexture = GFX.Game["objects/woodPlatform/" + texture];
            textures = new MTexture[fullTexture.Width / 8];
            for (int i = 0; i < textures.Length; i++) {
                textures[i] = fullTexture.GetSubtexture(i * 8, 0, 8, 8);
            }

            if (spawnedByOtherPlatform) {
                // this platform was spawned by another platform that spawned the moving platform. so don't manage the static mover!
                return;
            }

            // add a multi-node moving platform, pass the platform settings to it, and attach the bumper to it.
            StaticMover staticMover = new StaticMover() {
                OnMove = move => UpsideDownJumpthruOnMove(this, playerInteractingSolid, move)
            };
            Add(staticMover);
            animatingPlatform = new MultiNodeMovingPlatform(thisEntityData, thisOffset, otherPlatform => {
                if (otherPlatform != animatingPlatform) {
                    // another multi-node moving platform was spawned (because of the "count" setting), spawn another platform...
                    UpsideDownMovingPlatform otherUpsideDownPlatform = new UpsideDownMovingPlatform(thisEntityData, thisOffset);
                    otherUpsideDownPlatform.spawnedByOtherPlatform = true;
                    Scene.Add(otherUpsideDownPlatform);

                    // ... and attach it to that new platform.
                    StaticMover otherStaticMover = new StaticMover() {
                        OnMove = move => UpsideDownJumpthruOnMove(otherUpsideDownPlatform, otherUpsideDownPlatform.playerInteractingSolid, move)
                    };
                    otherUpsideDownPlatform.Add(otherStaticMover);
                    otherPlatform.AnimateObject(otherStaticMover, forcedTrackOffset: new Vector2(Width, Height) / 2f);
                }
            });
            animatingPlatform.AnimateObject(staticMover, forcedTrackOffset: new Vector2(Width, Height) / 2f);
            scene.Add(animatingPlatform);
        }

        // called when the platform moves, with the move amount
        public static void UpsideDownJumpthruOnMove(Entity platform, Solid playerInteractingSolid, Vector2 move) {
            if (platform.Scene == null) {
                // the platform isn't in the scene yet (initial offset is applied by the moving platform), so don't do collide checks and just move.
                platform.Position += move;
                playerInteractingSolid.MoveHNaive(move.X);
                playerInteractingSolid.MoveVNaive(move.Y);
                return;
            }

            bool playerHasToMove = false;

            if (platform.CollideCheckOutside<Player>(platform.Position + move) && Math.Sign(move.Y) == 1) {
                // the platform is pushing the player vertically, so we should have the solid push the player.
                playerHasToMove = true;
            }

            // move the platform..
            platform.Position += move;

            // back up the riders, because we don't want to mess up the static variable by moving a solid while moving another solid.
            HashSet<Actor> ridersBackup = new HashSet<Actor>(solidRiders);
            solidRiders.Clear();

            // move the hidden solid, making it actually solid if needed. When solid, it will push the player and carry them if they climb the platform.
            playerInteractingSolid.Collidable = playerHasToMove;
            playerInteractingSolid.MoveH(move.X);
            playerInteractingSolid.MoveV(move.Y);
            playerInteractingSolid.Collidable = false;

            // restore the riders
            solidRiders.Clear();
            foreach (Actor rider in ridersBackup) {
                solidRiders.Add(rider);
            }
        }

        public override void Render() {
            base.Render();

            Vector2 origin = new Vector2(8f, 8f);

            textures[3].Draw(Position, origin, Color.White, 1f, (float) Math.PI);
            for (int i = 8; i < Width - 8f; i += 8) {
                textures[1].Draw(Position + new Vector2(i, 0f), origin, Color.White, 1f, (float) Math.PI);
            }
            textures[0].Draw(Position + new Vector2(Width - 8f, 0f), origin, Color.White, 1f, (float) Math.PI);
            textures[2].Draw(Position + new Vector2(Width / 2f - 4f, 0f), origin, Color.White, 1f, (float) Math.PI);
        }
    }
}
