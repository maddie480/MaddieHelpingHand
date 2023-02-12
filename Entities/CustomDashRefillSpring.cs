using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomDashRefillSpring", "MaxHelpingHand/CustomDashRefillSpringLeft",
        "MaxHelpingHand/CustomDashRefillSpringRight", "MaxHelpingHand/CustomDashRefillSpringDown")]
    public class CustomDashRefillSpring : NoDashRefillSpring {
        private enum Mode { Set, Add, AddCapped }

        private readonly Mode mode;
        private readonly int dashCount;
        private readonly int dashCountCap;

        public CustomDashRefillSpring(EntityData data, Vector2 offset) : base(data, offset) {
            mode = data.Enum("mode", Mode.AddCapped);
            dashCount = data.Int("dashCount");
            dashCountCap = data.Int("dashCountCap");
        }

        protected override void RefillDashes(Player player) {
            switch (mode) {
                case Mode.Set:
                    player.Dashes = dashCount;
                    break;

                case Mode.Add:
                    player.Dashes += dashCount;
                    break;

                case Mode.AddCapped:
                    if (player.Dashes < dashCountCap) {
                        player.Dashes += dashCount;
                        player.Dashes = Math.Min(player.Dashes, dashCountCap);
                    }
                    break;
            }

            // In case someone uses a negative dashCount...
            player.Dashes = Math.Max(player.Dashes, 0);
        }
    }
}
