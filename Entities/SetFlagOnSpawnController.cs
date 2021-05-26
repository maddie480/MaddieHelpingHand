using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/SetFlagOnSpawnController")]
    public class SetFlagOnSpawnController : Entity {
        public SetFlagOnSpawnController(EntityData data, Vector2 offset) : base() {
            // we aren't supposed to access the level this early on, but that's a good way to be sure to set the flag
            // before Added or Awake is called on any entity in the level.
            Level level = Engine.Scene as Level;
            if (level == null) {
                level = (Engine.Scene as LevelLoader)?.Level;
            }

            level?.Session.SetFlag(data.Attr("flag"), data.Bool("enable"));
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // this entity only has work to do on spawn, it can disappear after that.
            RemoveSelf();
        }
    }
}
