using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // This entity just allows getting rid of a persistent seeker barrier color controller.
    [CustomEntity("MaxHelpingHand/SeekerBarrierColorControllerDisabler")]
    class SeekerBarrierColorControllerDisabler : Entity {
        public SeekerBarrierColorControllerDisabler(EntityData data, Vector2 offset) : base(data.Position + offset) {
            MaxHelpingHandModule.Instance.Session.SeekerBarrierCurrentColors = null;
        }
    }
}
