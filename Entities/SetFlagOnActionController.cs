using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/SetFlagOnActionController")]
    public class SetFlagOnActionController : Entity {
        private enum Action { OnGround, InAir, Climb, Dash, Swim }

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
                }
            }

            if (inverted) {
                isDoingAction = !isDoingAction;
            }

            SceneAs<Level>().Session.SetFlag(flag, isDoingAction);
        }
    }
}
