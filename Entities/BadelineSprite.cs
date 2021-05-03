using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/BadelineSprite")]
    static class BadelineSprite {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            // there is literally something made for this in vanilla... but it cannot be placed as an entity.
            BadelineDummy dummy = new BadelineDummy(entityData.Position + offset);
            if (!entityData.Bool("left")) {
                dummy.Sprite.Scale.X = 1;
            }
            if (!entityData.Bool("floating")) {
                dummy.Floatness = 0f;
                dummy.Sprite.Play("idle");
            }
            return dummy;
        }
    }
}
