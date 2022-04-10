using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomSeekerBarrier")]
    [TrackedAs(typeof(SeekerBarrier))]
    public class CustomSeekerBarrier : SeekerBarrier {
        internal class Renderer : SeekerBarrierRenderer {
            internal Color color;
            internal float transparency;
        }

        private SeekerBarrierRenderer renderer;

        internal Color particleColor;
        internal float particleTransparency;
        internal float particleDirection;
        internal bool wavy;

        public CustomSeekerBarrier(EntityData data, Vector2 offset) : base(data, offset) {
            renderer = new Renderer() {
                Tag = Tags.TransitionUpdate, // get rid of the Global tag
                Depth = 1, // vanilla is 0, this makes it dependent on loading order
                color = Calc.HexToColor(data.Attr("color", "FFFFFF")),
                transparency = data.Float("transparency", 0.15f)
            };

            particleColor = Calc.HexToColor(data.Attr("particleColor", "FFFFFF"));
            particleTransparency = data.Float("particleTransparency", 0.5f);
            particleDirection = data.Float("particleDirection", 0f);
            wavy = data.Bool("wavy", defaultValue: true);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // track the barrier on our own renderer, instead of the vanilla global one.
            scene.Tracker.GetEntity<SeekerBarrierRenderer>().Untrack(this);
            scene.Add(renderer);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            renderer.Track(this);

            if (!SeekerBarrierColorController.HasControllerOnNextScreen() && scene.Entities.ToAdd.OfType<SeekerBarrierColorController>().Count() == 0) {
                // there is no seeker barrier color controller and we're not already adding one :pensive:
                // we need one, because that's the one tweaking the barriers.
                scene.Add(new SeekerBarrierColorController(new EntityData(), Vector2.Zero));
            }
        }
    }
}
