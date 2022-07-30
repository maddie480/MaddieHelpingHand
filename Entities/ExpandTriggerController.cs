using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ExpandTriggerController")]
    public class ExpandTriggerController : Entity {
        public ExpandTriggerController(EntityData data, Vector2 offset)
            : base(data.Position + offset) {
            base.Collider = new Hitbox(24f, 24f, -12f, -12f);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            foreach (Trigger item in CollideAll<Trigger>()) {
                item.Position = new Vector2(SceneAs<Level>().Bounds.Left, SceneAs<Level>().Bounds.Top - 24f);
                item.Collider.Width = SceneAs<Level>().Bounds.Width;
                item.Collider.Height = SceneAs<Level>().Bounds.Height + 32f;
            }
            RemoveSelf();
        }
    }
}
