using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomizableGlassBlockAreaController")]
    class CustomizableGlassBlockAreaController : CustomizableGlassBlockController {
        public CustomizableGlassBlockAreaController(EntityData data, Vector2 offset) : base(data, offset) {
            Collider = new Hitbox(data.Width, data.Height);
        }

        protected override IEnumerable<CustomizableGlassBlock> getGlassBlocksToAffect() {
            return CollideAll<CustomizableGlassBlock>().OfType<CustomizableGlassBlock>();
        }
    }
}
