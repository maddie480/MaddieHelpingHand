using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // a multi-node bumper, except it's actually just a static bumper attached to a multi-node moving platform because yes
    // ... I swear, this is code reusing, not a jank hack. :distracteline:
    [CustomEntity("MaxHelpingHand/MultiNodeBumper")]
    class MultiNodeBumper : BumperNotCoreMode {
        public static void Load() {
            On.Celeste.Bumper.UpdatePosition += onBumperWiggle;
        }

        public static void Unload() {
            On.Celeste.Bumper.UpdatePosition -= onBumperWiggle;
        }

        private static void onBumperWiggle(On.Celeste.Bumper.orig_UpdatePosition orig, Bumper self) {
            // please don't make my bumpers wiggle.
            if (!(self is MultiNodeBumper) && !(self is RotatingBumper)) {
                orig(self);
            }
        }

        private readonly EntityData thisEntityData;
        private readonly Vector2 thisOffset;

        private MultiNodeMovingPlatform animatingPlatform;
        private bool spawnedByOtherBumper = false;

        public MultiNodeBumper(EntityData data, Vector2 offset) : base(data.Position + offset, null, data.Bool("notCoreMode", false)) {
            thisEntityData = data;
            thisOffset = offset;
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (spawnedByOtherBumper) {
                // this bumper was spawned by another bumper that spawned the platform. so we have nothing to do!
                return;
            }

            // add a multi-node moving platform, pass the bumper settings to it, and attach the bumper to it.
            StaticMover staticMover = new StaticMover();
            Add(staticMover);
            animatingPlatform = new MultiNodeMovingPlatform(thisEntityData, thisOffset, otherPlatform => {
                if (otherPlatform != animatingPlatform) {
                    // another multi-node moving platform was spawned (because of the "count" setting), spawn another bumper...
                    MultiNodeBumper otherBumper = new MultiNodeBumper(thisEntityData, thisOffset);
                    otherBumper.spawnedByOtherBumper = true;
                    Scene.Add(otherBumper);

                    // ... and attach it to that new platform.
                    StaticMover otherStaticMover = new StaticMover();
                    otherBumper.Add(otherStaticMover);
                    otherPlatform.AnimateObject(otherStaticMover);
                }
            });
            animatingPlatform.AnimateObject(staticMover);
            scene.Add(animatingPlatform);
        }
    }
}
