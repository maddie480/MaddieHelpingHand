using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/FlagTouchSwitchWall")]
    public class FlagTouchSwitchWall : FlagTouchSwitch {
        protected override Vector2 IconPosition => new Vector2(Width / 2, Height / 2);

        public FlagTouchSwitchWall(EntityData data, Vector2 offset) : base(data, offset) {
        }

        protected override void setUpCollision(EntityData data) {
            Collider = new Hitbox(data.Width, data.Height);

            if (data.Bool("playerCanActivate", defaultValue: true)) {
                Add(new PlayerCollider(onPlayer, null, new Hitbox(data.Width, data.Height)));
            }

            Add(new HoldableCollider(onHoldable, new Hitbox(data.Width, data.Height)));
            Add(new SeekerCollider(onSeeker, new Hitbox(data.Width, data.Height)));
        }

        protected override void renderBorder() {
            Draw.HollowRect(X - 1, Y - 1, Width + 2, Height + 2, new Color(icon.Color.R, icon.Color.G, icon.Color.B, (int) (0.7f * 255)));
            Draw.Rect(X + 1, Y + 1, Width - 2, Height - 2, Color.Lerp(icon.Color, Calc.HexToColor("0a0a0a"), 0.5f) * 0.3f);
        }
    }
}
