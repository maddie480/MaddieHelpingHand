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
        private static bool gravityHelperHooked = false;

        public static void LoadMods() {
            if (!gravityHelperHooked && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "GravityHelper", Version = new Version(1, 2, 8) })) {
                // it's not really a Gravity Helper hook, but it checks for Gravity Helper stuff,
                // and therefore will go boom if Gravity Helper is not loaded.
                On.Celeste.JumpThru.MoveHExact += jumpThruMoveHExact;
                gravityHelperHooked = true;
            }
        }

        public static void Unload() {
            if (gravityHelperHooked) {
                On.Celeste.JumpThru.MoveHExact -= jumpThruMoveHExact;
                gravityHelperHooked = false;
            }
        }

        // settings
        private readonly EntityData thisEntityData;
        private readonly Vector2 thisOffset;
        private readonly string texture;
        private readonly int width;

        private MTexture[] textures;

        // the moving platform that makes that moving platform move :theoreticalwoke:
        private MultiNodeMovingPlatform animatingPlatform;
        private bool spawnedByOtherPlatform = false;

        private JumpThru gravityJumpthru;

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

        private static void animatePlatforms(UpsideDownMovingPlatformGravityHelper self, MultiNodeMovingPlatform animatingPlatform, JumpThru gravityJumpthru) {
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

        private static void jumpThruMoveHExact(On.Celeste.JumpThru.orig_MoveHExact orig, JumpThru self, int move) {
            if (!(self is GravityJumpThru jumpThru)
                || !(jumpThru.Get<StaticMover>()?.Platform is MultiNodeMovingPlatform platform)
                || !platform.giveHorizontalBoost) {

                // do not change the vanilla behavior
                orig(self, move);
                return;
            }

            // A slightly edited version of vanilla's MoveHExact, edited to give lift boost like MoveVExact does.
            // (pulled from Vortex Helper's Attached Jump Thrus)
            // This uses the invisible moving platform's lift speed, since the attached jumpthrus get moved by pixel
            // and give lift boosts by multiples of 60 only.

            if (self.Collidable) {
                foreach (Actor entity in self.Scene.Tracker.GetEntities<Actor>()) {
                    if (entity.IsRiding(self)) {
                        self.Collidable = false;

                        if (entity.TreatNaive) {
                            entity.NaiveMove(Vector2.UnitX * move);
                        } else {
                            entity.MoveHExact(move);
                        }

                        entity.LiftSpeed = platform.LiftSpeed;
                        self.Collidable = true;
                        Console.WriteLine(entity.LiftSpeed);
                    }
                }
            }

            self.X += move;
            self.MoveStaticMovers(Vector2.UnitX * move);
        }
    }
}
