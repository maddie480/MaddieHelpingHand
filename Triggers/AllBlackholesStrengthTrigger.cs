using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/AllBlackholesStrengthTrigger")]
    class AllBlackholesStrengthTrigger : Trigger {
        private BlackholeBG.Strengths strength;

        public AllBlackholesStrengthTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            strength = data.Enum("strength", BlackholeBG.Strengths.Mild);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);
            foreach (BlackholeBG blackhole in (Scene as Level).Background.GetEach<BlackholeBG>()) {
                blackhole.NextStrength(Scene as Level, strength);
            }
        }
    }
}
