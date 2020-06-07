using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    class GroupedDustTriggerSpikes : TriggerSpikes {
        public static void Load() {
            On.Celeste.TriggerSpikes.PlayerCheck += onTriggerSpikesPlayerCheck;
        }

        public static void Unload() {
            On.Celeste.TriggerSpikes.PlayerCheck -= onTriggerSpikesPlayerCheck;
        }

        public GroupedDustTriggerSpikes(EntityData data, Vector2 offset, Directions dir) : base(data, offset, dir) {
            new DynData<TriggerSpikes>(this)["grouped"] = true;
        }

        private static bool onTriggerSpikesPlayerCheck(On.Celeste.TriggerSpikes.orig_PlayerCheck orig, TriggerSpikes self, int spikeIndex) {
            DynData<TriggerSpikes> selfData = new DynData<TriggerSpikes>(self);
            if (selfData.Data.ContainsKey("grouped") && selfData.Get<bool>("grouped")) {
                // let's pretend the player is pressing every trigger spike at once when they collide with the spikes.
                return self.CollideCheck<Player>();
            }

            // this is a vanilla trigger spike: don't mod anything!
            return orig(self, spikeIndex);
        }
    }
}
