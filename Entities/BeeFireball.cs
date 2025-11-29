using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// Fireballs reskinned as bees that turn around, as seen in Strawberry Jam.
    /// </summary>
    [CustomEntity("MaxHelpingHand/BeeFireball")]
    public class BeeFireball : FireBall {
        private static ParticleType P_Noop;

        public static void LoadContent() {
            P_Noop = new ParticleType(P_FireTrail) {
                Color = Color.White * 0f,
                Color2 = Color.White * 0f
            };
        }

        private bool isFacingLeft;
        private bool isFacingLeftAtStartOfTrack;

        public BeeFireball(EntityData data, Vector2 offset) : this(data.NodesWithPosition(offset), data.Int("amount", 1), 0, data.Float("offset"), data.Float("speed", 1f)) { }

        public BeeFireball(Vector2[] nodes, int amount, int index, float offset, float speedMult) : base(nodes, amount, index, offset, speedMult, notCoreMode: true) {
            // replace fireball sprites with bee sprites
            sprite = Get<Sprite>();
            GFX.SpriteBank.CreateOn(sprite, "MaxHelpingHand_beeFireball");
            sprite.Play("idle", restart: false, randomizeFrame: true);

            // this fireball doesn't support core mode switching, so just make sure.
            Remove(Get<CoreModeListener>());

            // lower the hitbox a bit to match the sprite.
            Collider.CenterY += 2;
        }

        // this will be relinked to Entity.Added instead of FireBall.Added.
        [MonoModLinkTo("Monocle.Entity", "System.Void Added(Monocle.Scene)")]
        [MonoModForceCall]
        public void base_Added(Scene scene) {
            throw new NotImplementedException("WTF? MonoModLinkTo is supposed to have relinked calls to this method!");
        }


        // Added method similar to vanilla, except iceMode is forced to false, and amount > 1 instantiates more bee fireballs.
        public override void Added(Scene scene) {
            base_Added(scene);

            iceMode = false;
            speedMult = 1;
            if (index == 0) {
                for (int i = 1; i < amount; i++) {
                    Scene.Add(new BeeFireball(nodes, amount, i, offset, mult));
                }
            }

            trackSfx?.RemoveSelf();
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // what will be the initial facing? determine it by computing (position at percent + 0.01) - (position at percent).
            float initPercent = percent;
            float firstMoveX = GetPercentPosition((initPercent + 0.01f) % 1f).X
                - GetPercentPosition(initPercent).X;

            if (firstMoveX < 0) {
                // facing left!
                sprite.Scale.X = -1;
                isFacingLeft = true;
            }

            // check if the bee is facing left at the start of the track (if it is moving left between 0% and 1% of the track).
            isFacingLeftAtStartOfTrack = GetPercentPosition(0f).X > GetPercentPosition(0.01f).X;
        }

        public override void Update() {
            Vector2 prev = Position;
            float prevPercent = percent;

            // modify the fire particles
            ParticleType prevFireTrail = P_FireTrail;
            P_FireTrail = P_Noop;

            // run the vanilla method that might emit particles
            base.Update();

            // change the fire particles back
            P_FireTrail = prevFireTrail;

            // determine if we should be facing left by ... determining if we just moved left.
            float moveX = Position.X - prev.X;

            // if we didn't move horizontally, don't change the facing.
            if (moveX == 0) {
                return;
            }

            // if the percentage was reset, this means we teleported back to the start of the track...
            if (percent < prevPercent) {
                // ... so we need to adjust the facing immediately.
                sprite.Play("idle");
                sprite.Scale.X = isFacingLeftAtStartOfTrack ? -1 : 1;
                isFacingLeft = isFacingLeftAtStartOfTrack;
                return;
            }

            bool shouldFaceLeft = (moveX < 0);

            if (shouldFaceLeft == isFacingLeft) {
                if (sprite.CurrentAnimationID == "idle") {
                    // ensure facing is enforced (in particular if turning animation just finished).
                    sprite.Scale.X = isFacingLeft ? -1 : 1;
                }
            } else {
                // we should turn around!
                // the sprite should be flipped if we should face **right**, because the animation is for turning **right to left**.
                sprite.Scale.X = shouldFaceLeft ? 1 : -1;

                if (sprite.CurrentAnimationID == "idle") {
                    // start rotating
                    sprite.Play("rotate");
                } else {
                    // rotating while already rotating: if there are 8 frames and we are at frame 6, restart playing the animation from frame 2 for example.
                    sprite.SetAnimationFrame(sprite.CurrentAnimationTotalFrames - sprite.CurrentAnimationFrame);
                }

                isFacingLeft = shouldFaceLeft;
            }
        }
    }
}
