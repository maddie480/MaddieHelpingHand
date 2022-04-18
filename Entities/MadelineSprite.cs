using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/MadelineSprite")]
    public class MadelineSprite : Player {
        public static void Load() {
            On.Celeste.Player.RefillDash += onRefillDash;
        }

        public static void Unload() {
            On.Celeste.Player.RefillDash -= onRefillDash;
        }

        private readonly int dashCount;

        public MadelineSprite(EntityData data, Vector2 offset) : base(data.Position + offset, data.Bool("hasBackpack") ? PlayerSpriteMode.Madeline : PlayerSpriteMode.MadelineNoBackpack) {
            StateMachine.State = StDummy;

            if (data.Bool("left")) {
                Facing = Facings.Left;
            }

            dashCount = data.Int("dashCount");

            DummyGravity = false;
            DummyAutoAnimate = false;
            IntroType = IntroTypes.None;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Dashes = dashCount;
        }

        private static bool onRefillDash(On.Celeste.Player.orig_RefillDash orig, Player self) {
            if (self is MadelineSprite) {
                return false;
            }
            return orig(self);
        }
    }
}
