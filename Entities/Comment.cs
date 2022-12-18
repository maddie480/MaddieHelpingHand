using Celeste.Mod.Entities;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/Comment")]
    public class Comment : Entity {
        public Comment() { }

        public override void Added(Scene scene) {
            RemoveSelf();
        }
    }
}
