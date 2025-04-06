using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    /**
     * A combination of AnimatedParallax and HdParallax.
     * This class extends AnimatedParallax, and most of the HD parallax stuff
     * is handled by hooks in the HdParallax class.
     */
    public class AnimatedHdParallax : AnimatedParallax {
        public AnimatedHdParallax(MTexture texture) : base(texture) {
        }

        public override void Render(Scene scene) {
            // don't render the usual way!
        }
    }
}
