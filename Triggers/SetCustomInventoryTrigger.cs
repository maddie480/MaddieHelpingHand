using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/SetCustomInventoryTrigger")]
    public class SetCustomInventoryTrigger : Trigger {
        private readonly int dashes;
        private readonly bool dreamDash;
        private readonly bool groundRefills;
        private readonly bool backpack;

        public SetCustomInventoryTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            dashes = data.Int("dashes");
            dreamDash = data.Bool("dreamDash");
            groundRefills = data.Bool("groundRefills");
            backpack = data.Bool("backpack");
        }

        public override void OnEnter(Player player) {
            Session session = (Scene as Level).Session;
            bool hadBackpack = session.Inventory.Backpack;

            session.Inventory = new PlayerInventory(dashes, dreamDash, backpack, !groundRefills);

            if (!SaveData.Instance.Assists.PlayAsBadeline && hadBackpack != backpack) {
                player.ResetSprite(backpack ? PlayerSpriteMode.Madeline : PlayerSpriteMode.MadelineNoBackpack);
            }
        }
    }
}
