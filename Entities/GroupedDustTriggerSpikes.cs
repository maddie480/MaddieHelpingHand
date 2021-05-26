using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    public class GroupedDustTriggerSpikes : TriggerSpikes {
        public static void Load() {
            On.Celeste.TriggerSpikes.GetPlayerCollideIndex += onTriggerSpikesGetPlayerCollideIndex;
        }

        public static void Unload() {
            On.Celeste.TriggerSpikes.GetPlayerCollideIndex -= onTriggerSpikesGetPlayerCollideIndex;
        }

        public GroupedDustTriggerSpikes(EntityData data, Vector2 offset, Directions dir) : base(data, offset, dir) {
            new DynData<TriggerSpikes>(this)["grouped"] = true;
        }

        private static void onTriggerSpikesGetPlayerCollideIndex(On.Celeste.TriggerSpikes.orig_GetPlayerCollideIndex orig,
            TriggerSpikes self, Player player, out int minIndex, out int maxIndex) {

            orig(self, player, out minIndex, out maxIndex);

            DynData<TriggerSpikes> selfData = new DynData<TriggerSpikes>(self);
            if (selfData.Data.ContainsKey("grouped") && selfData.Get<bool>("grouped")) {
                int spikeCount = selfData.Get<int>("size") / 4;

                if (maxIndex >= 0 && minIndex < spikeCount) {
                    // let's pretend the player is pressing every trigger spike at once.
                    minIndex = 0;
                    maxIndex = spikeCount - 1;
                }
            }
        }
    }
}
