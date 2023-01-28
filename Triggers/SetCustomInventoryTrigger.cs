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
            dashes = (int)data.Values["dashes"];
            dreamDash = (bool)data.Values["dreamDash"];
            groundRefills = (bool)data.Values["groundRefills"];
            backpack = (bool)data.Values["backpack"];
        }

        public override void OnEnter(Player player) {
            (base.Scene as Level).Session.Inventory = new PlayerInventory(dashes, dreamDash, backpack, !groundRefills);
            player.ResetSprite(backpack ? PlayerSpriteMode.Madeline : PlayerSpriteMode.MadelineNoBackpack);
        }
    }
}