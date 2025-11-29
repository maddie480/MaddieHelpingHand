using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/LitBlueTorch")]
    public class LitBlueTorch : Torch {
        public LitBlueTorch(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id) { }

        public override void Added(Scene scene) {
            // by turning on startLit in Added but not in the constructor, we make the torch blue instead of yellow.
            startLit = true;

            base.Added(scene);
        }
    }
}