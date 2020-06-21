using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // This entity just allows getting rid of a persistent rainbow spinner color controller.
    [CustomEntity("MaxHelpingHand/RainbowSpinnerColorControllerDisabler")]
    class RainbowSpinnerColorControllerDisabler : Entity {
        public RainbowSpinnerColorControllerDisabler(EntityData data, Vector2 offset) : base(data.Position + offset) {
            MaxHelpingHandModule.Instance.Session.RainbowSpinnerCurrentColors = null;
        }
    }
}
