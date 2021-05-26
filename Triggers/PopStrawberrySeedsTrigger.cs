using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/PopStrawberrySeedsTrigger")]
    public class PopStrawberrySeedsTrigger : Trigger {
        public PopStrawberrySeedsTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void OnStay(Player player) {
            base.OnStay(player);

            foreach (Follower follower in player.Leader.Followers.ToArray()) {
                if (follower.Entity is StrawberrySeed &&
                    !(follower.Entity is MultiRoomStrawberrySeed /* support is more complicated and will only be added on request */)) {

                    player.Leader.LoseFollower(follower);
                }
            }
        }
    }
}
