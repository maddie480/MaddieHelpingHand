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

        private readonly bool triggerIfSameDirection;
        private readonly bool killIfSameDirection;

        private bool triggered = false;

        public GroupedDustTriggerSpikes(EntityData data, Vector2 offset, Directions dir) : base(data, offset, dir) {
            triggerIfSameDirection = data.Bool("triggerIfSameDirection", defaultValue: false);
            killIfSameDirection = data.Bool("killIfSameDirection", defaultValue: triggerIfSameDirection);
        }

        private static void onTriggerSpikesGetPlayerCollideIndex(On.Celeste.TriggerSpikes.orig_GetPlayerCollideIndex orig,
            TriggerSpikes self, Player player, out int minIndex, out int maxIndex) {

            // if we want the spikes to trigger when the player is going in the same direction, we should set the speed to 0 because a 0 speed always triggers spikes.
            Vector2 initialPlayerSpeed = player.Speed;
            bool triggerIfSameDirection = (self is GroupedDustTriggerSpikes grouped && grouped.evenIfSameDirection());
            if (triggerIfSameDirection) {
                player.Speed = Vector2.Zero;
            }

            orig(self, player, out minIndex, out maxIndex);

            // ... don't forget to restore the speed afterwards though!
            if (triggerIfSameDirection) {
                player.Speed = initialPlayerSpeed;
            }

            if (self is GroupedDustTriggerSpikes groupedSpikes) {
                int spikeCount = new DynData<TriggerSpikes>(self).Get<int>("size") / 4;

                if (maxIndex >= 0 && minIndex < spikeCount) {
                    // let's pretend the player is pressing every trigger spike at once.
                    minIndex = 0;
                    maxIndex = spikeCount - 1;

                    // the spikes have been triggered.
                    groupedSpikes.triggered = true;
                }
            }
        }

        private bool evenIfSameDirection() {
            if (triggered) {
                return killIfSameDirection;
            } else {
                return triggerIfSameDirection;
            }
        }
    }
}
