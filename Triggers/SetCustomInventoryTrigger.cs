using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/SetCustomInventoryTrigger")]
    public class SetCustomInventoryTrigger : Trigger {
        private int dashes = 1;
        private bool dreamDash = false;
        private bool groundRefills = true;
        private bool backpack = true;

        public SetCustomInventoryTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            dashes = data.Int("dashes");
            dreamDash = data.Bool("dreamDash");
            groundRefills = data.Bool("groundRefills");
            backpack = data.Bool("backpack");
        }

        public override void OnEnter(Player player) {
            (Scene as Level).Session.Inventory = new PlayerInventory(dashes, dreamDash, backpack, !groundRefills);

            if (!SaveData.Instance.Assists.PlayAsBadeline) {
                player.ResetSprite(backpack ? PlayerSpriteMode.Madeline : PlayerSpriteMode.MadelineNoBackpack);
            }
        }
    }
}
