using Celeste.Mod.Entities;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// A controller creating a horizontal room wrap effect in the room it is placed in.
    /// Pulled straight from Celsius by 0x0ade.
    /// </summary>
    [CustomEntity("MaxHelpingHand/HorizontalRoomWrapController")]
    public class HorizontalRoomWrapController : Entity {
        public override void Update() {
            base.Update();

            Camera camera = SceneAs<Level>().Camera;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                if (player.Left > camera.Right + 12f) {
                    // right -> left wrap
                    player.Right = camera.Left - 4f;
                } else if (player.Right < camera.Left - 4f) {
                    // left -> right wrap
                    player.Left = camera.Right + 12f;
                }
            }
        }
    }

}
