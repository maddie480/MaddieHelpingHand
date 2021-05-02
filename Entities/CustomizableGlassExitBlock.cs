using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // The forbidden crossing between glass blocks and exit blocks.
    [CustomEntity("MaxHelpingHand/CustomizableGlassExitBlock")]
    class CustomizableGlassExitBlock : CustomizableGlassBlock {
        private EffectCutout cutout;

        public CustomizableGlassExitBlock(EntityData data, Vector2 offset) : base(data, offset) {
            Add(cutout = new EffectCutout());
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // make sure we are going to be awoken first (before other glass blocks at least).
            scene.Entities.ToAdd.Remove(this);
            scene.Entities.ToAdd.Insert(0, this);
        }

        public override void Awake(Scene scene) {
            if (CollideCheck<Player>()) {
                cutout.Alpha = Alpha = 0f;
                Collidable = false;
            }

            base.Awake(scene);
        }

        public override void Update() {
            base.Update();
            if (Collidable) {
                cutout.Alpha = Alpha = Calc.Approach(Alpha, 1f, Engine.DeltaTime);
            } else if (!CollideCheck<Player>()) {
                Collidable = true;
                Audio.Play("event:/game/general/passage_closed_behind", Center);
                foreach (CustomizableGlassBlock block in Scene.Tracker.GetEntities<CustomizableGlassBlock>()) {
                    block.GlassExitBlockSolidified(this);
                }
            }
        }
    }
}
