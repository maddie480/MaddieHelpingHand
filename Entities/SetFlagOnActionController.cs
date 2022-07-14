using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/SetFlagOnActionController")]
    public class SetFlagOnActionController : Entity {
        private enum Action { OnGround, InAir, Climb, Dash, Swim, HoldItem, NoDashLeft, FullDashes, NoStaminaLeft, LowStamina, FullStamina }

        private readonly Action action;
        private readonly string flag;
        private readonly bool inverted;

        public SetFlagOnActionController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            action = data.Enum<Action>("action");
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
        }

        public override void Update() {
            base.Update();

            bool isDoingAction = false;

            Player p = Scene.Tracker.GetEntity<Player>();

            if (p != null) {
                switch (action) {
                    case Action.OnGround:
                        isDoingAction = p.OnGround();
                        break;

                    case Action.InAir:
                        isDoingAction = !p.OnGround();
                        break;

                    case Action.Climb:
                        isDoingAction = (p.StateMachine.State == Player.StClimb);
                        break;

                    case Action.Dash:
                        isDoingAction = (p.StateMachine.State == Player.StDash);
                        break;

                    case Action.Swim:
                        isDoingAction = (p.StateMachine.State == Player.StSwim);
                        break;

                    case Action.HoldItem:
                        isDoingAction = (p.Holding != null);
                        break;

                    case Action.NoDashLeft:
                        isDoingAction = (p.Dashes == 0);
                        break;

                    case Action.FullDashes:
                        isDoingAction = (p.Dashes == p.MaxDashes);
                        break;

                    case Action.NoStaminaLeft:
                        isDoingAction = (p.Stamina <= 0f);
                        break;

                    case Action.LowStamina:
                        isDoingAction = (p.Stamina <= Player.ClimbTiredThreshold);
                        break;

                    case Action.FullStamina:
                        isDoingAction = (p.Stamina >= Player.ClimbMaxStamina);
                        break;
                }
            }

            if (inverted) {
                isDoingAction = !isDoingAction;
            }

            SceneAs<Level>().Session.SetFlag(flag, isDoingAction);
        }
    }
}
