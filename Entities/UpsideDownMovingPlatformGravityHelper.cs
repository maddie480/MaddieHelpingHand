using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using GravityJumpThru = Celeste.Mod.GravityHelper.Entities.UpsideDownJumpThru;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // This entity spawns a multi-node moving platform, a Gravity Helper upside-down jumpthru, attaches both,
    // and just takes care of the rendering afterwards. :sparkles:
    [CustomEntity("MaxHelpingHand/UpsideDownMovingPlatformGravityHelper")]
    public class UpsideDownMovingPlatformGravityHelper : Entity {
        // settings
        private readonly EntityData thisEntityData;
        private readonly Vector2 thisOffset;
        private readonly string texture;
        private readonly int width;

        private MTexture[] textures;

        // the moving platform that makes that moving platform move :theoreticalwoke:
        private MultiNodeMovingPlatform animatingPlatform;
        private bool spawnedByOtherPlatform = false;

        private GravityJumpThru gravityJumpthru;

        public UpsideDownMovingPlatformGravityHelper(EntityData data, Vector2 offset) : base(data.Position + offset) {
            gravityJumpthru = new GravityJumpThru(new EntityData() {
                Position = data.Position,
                Width = data.Width,
                Values = new Dictionary<string, object>() {
                    { "attached", true },
                    { "texture", "MaxHelpingHand/invisible" }
                }
            }, offset);
            gravityJumpthru.Get<StaticMover>().SolidChecker = null;

            thisEntityData = data;
            thisOffset = offset;
            texture = data.Attr("texture", "default");
            width = data.Width;

            Add(new StaticMover());
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // add the Gravity Helper upside-down jumpthru to the scene, now that we have it.
            scene.Add(gravityJumpthru);

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

            // add a multi-node moving platform, pass the platform settings to it, and attach the jumpthru to it.
            animatingPlatform = new MultiNodeMovingPlatform(thisEntityData, thisOffset, otherPlatform => {
                if (otherPlatform != animatingPlatform) {
                    // another multi-node moving platform was spawned (because of the "count" setting), spawn another platform...
                    UpsideDownMovingPlatformGravityHelper otherUpsideDownPlatform = new UpsideDownMovingPlatformGravityHelper(thisEntityData, thisOffset);
                    otherUpsideDownPlatform.spawnedByOtherPlatform = true;
                    Scene.Add(otherUpsideDownPlatform);

                    // ... and attach it to that new platform.
                    StaticMover otherStaticMover = new StaticMover();
                    otherUpsideDownPlatform.Add(otherStaticMover);
                    animatePlatforms(otherUpsideDownPlatform, otherPlatform, otherUpsideDownPlatform.gravityJumpthru);
                }
            });
            scene.Add(animatingPlatform);
            animatePlatforms(this, animatingPlatform, gravityJumpthru);
        }

        private static void animatePlatforms(UpsideDownMovingPlatformGravityHelper self, MultiNodeMovingPlatform animatingPlatform, GravityJumpThru gravityJumpthru) {
            StaticMover staticMover = self.Get<StaticMover>();
            StaticMover gravityMover = gravityJumpthru.Get<StaticMover>();

            animatingPlatform.AnimateObject(staticMover);
            animatingPlatform.AnimateObject(gravityMover, forcedTrackOffset: new Vector2(self.width, 8) / 2f);
            gravityMover.OnAttach(animatingPlatform);
            gravityMover.Platform = animatingPlatform;

            animatingPlatform.sinkingDir = -1;
        }

        public override void Render() {
            base.Render();

            Vector2 origin = new Vector2(8f, 8f);

            textures[3].Draw(Position, origin, Color.White, 1f, (float) Math.PI);
            for (int i = 8; i < width - 8f; i += 8) {
                textures[1].Draw(Position + new Vector2(i, 0f), origin, Color.White, 1f, (float) Math.PI);
            }
            textures[0].Draw(Position + new Vector2(width - 8f, 0f), origin, Color.White, 1f, (float) Math.PI);
            textures[2].Draw(Position + new Vector2(width / 2f - 4f, 0f), origin, Color.White, 1f, (float) Math.PI);
        }
    }
}
