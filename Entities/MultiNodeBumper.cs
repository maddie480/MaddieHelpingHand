using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // a multi-node bumper, except it's actually just a static bumper attached to a multi-node moving platform because yes
    // ... I swear, this is code reusing, not a jank hack. :distracteline:
    [CustomEntity("MaxHelpingHand/MultiNodeBumper")]
    class MultiNodeBumper : Bumper {
        public static void Load() {
            On.Celeste.Bumper.UpdatePosition += onBumperWiggle;
        }

        public static void Unload() {
            On.Celeste.Bumper.UpdatePosition -= onBumperWiggle;
        }

        private static void onBumperWiggle(On.Celeste.Bumper.orig_UpdatePosition orig, Bumper self) {
            // please don't make my bumpers wiggle.
            if (!(self is MultiNodeBumper)) {
                orig(self);
            }
        }

        private readonly EntityData thisEntityData;
        private readonly Vector2 thisOffset;

        public MultiNodeBumper(EntityData data, Vector2 offset) : base(data.Position + offset, null) {
            thisEntityData = data;
            thisOffset = offset;
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // add a multi-node moving platform, pass the bumper settings to it, and attach the bumper to it.
            StaticMover staticMover = new StaticMover();
            Add(staticMover);
            MultiNodeMovingPlatform animatingPlatform = new MultiNodeMovingPlatform(thisEntityData, thisOffset);
            animatingPlatform.AnimateObject(staticMover);
            scene.Add(animatingPlatform);
        }
    }
}
