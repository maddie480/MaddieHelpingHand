using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/FlagToggleCameraOffsetTrigger")]
    static class FlagToggleCameraOffsetTrigger {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            Trigger trigger = new CameraOffsetTrigger(entityData, offset);
            trigger.Add(new FlagToggleComponent(entityData.Attr("flag"), entityData.Bool("inverted")));
            return trigger;
        }
    }
}
