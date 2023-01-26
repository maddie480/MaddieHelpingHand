using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/SetCustomInventoryTrigger")]
    public class SetCustomInventoryTrigger : Trigger {
        private int dashes = 1;
        private bool dream_dash = false;
        private bool ground_refills = true;
        private bool backpack = true;

        public SetCustomInventoryTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            dashes = (int)data.Values["dashes"];
            dream_dash = (bool)data.Values["dream_dash"];
            ground_refills = (bool)data.Values["ground_refills"];
            backpack = (bool)data.Values["backpack"];
        }

        public override void OnEnter(Player player) {
            (base.Scene as Level).Session.Inventory = new PlayerInventory(dashes, dream_dash, backpack, !ground_refills);
        }
    }
}