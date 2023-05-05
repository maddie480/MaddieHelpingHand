using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // Secret Berry, except not so secret
    [CustomEntity("MaxHelpingHand/CustomizableBerry")]
    [RegisterStrawberry(tracked: true, blocksCollection: false)]
    public class CustomizableBerry : SecretBerry {
        public CustomizableBerry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {
        }
    }
}
