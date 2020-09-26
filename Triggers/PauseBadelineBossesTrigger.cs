using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/PauseBadelineBossesTrigger")]
    class PauseBadelineBossesTrigger : Trigger {
        public PauseBadelineBossesTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void OnStay(Player player) {
            base.OnStay(player);

            // kick the attack coroutines from Badeline bosses so that they don't attack anymore.
            foreach (FinalBoss badelineBoss in Scene.Tracker.GetEntities<FinalBoss>()) {
                DynData<FinalBoss> bossData = new DynData<FinalBoss>(badelineBoss);
                if (badelineBoss.Sprite.CurrentAnimationID == "idle") {
                    badelineBoss.Remove(bossData.Get<Coroutine>("attackCoroutine"));
                }
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if (!player.Dead) {
                // add the attack coroutines back.
                foreach (FinalBoss badelineBoss in Scene.Tracker.GetEntities<FinalBoss>()) {
                    Coroutine attackCoroutine = new DynData<FinalBoss>(badelineBoss).Get<Coroutine>("attackCoroutine");
                    if (!badelineBoss.Components.Contains(attackCoroutine)) {
                        badelineBoss.Add(attackCoroutine);
                    }
                }
            }
        }
    }
}
