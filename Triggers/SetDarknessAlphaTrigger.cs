using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/SetDarknessAlphaTrigger")]
    public class SetDarknessAlphaTrigger : Trigger {
        private float value;

        public SetDarknessAlphaTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            value = data.Float("value");
        }

        public override void OnEnter(Player player) {
            Level level = Scene as Level;

            level.Lighting.Alpha = value;
            level.Session.LightingAlphaAdd = value - level.BaseLightingAlpha;
        }
    }
}
