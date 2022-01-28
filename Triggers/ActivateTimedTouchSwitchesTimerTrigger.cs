using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/ActivateTimedTouchSwitchesTimerTrigger")]
    [Tracked]
    public class ActivateTimedTouchSwitchesTimerTrigger : Trigger {
        private Dictionary<Entity, Coroutine> timedTouchSwitches;

        public static void Load() {
            On.Celeste.TouchSwitch.TurnOn += onTouchSwitchTurnOn;
        }

        public static void Unload() {
            On.Celeste.TouchSwitch.TurnOn -= onTouchSwitchTurnOn;
        }

        private static void onTouchSwitchTurnOn(On.Celeste.TouchSwitch.orig_TurnOn orig, TouchSwitch self) {
            Dictionary<Entity, Coroutine> timedTouchSwitches = self.Scene?.Tracker.GetEntity<ActivateTimedTouchSwitchesTimerTrigger>()?.timedTouchSwitches;

            if (timedTouchSwitches != null && timedTouchSwitches.ContainsKey(self)) {
                // reactivate this touch switch right now, in order not to block the activation animation.
                self.Add(timedTouchSwitches[self]);
                timedTouchSwitches.Remove(self);
            }

            orig(self);
        }

        public ActivateTimedTouchSwitchesTimerTrigger(EntityData data, Vector2 offset) : base(data, offset) { }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (timedTouchSwitches == null) {
                // before starting the timed touch switches... we should stop them.
                // they use a coroutine to make the time tick down, so we can just take that coroutine and remove it.
                // also store it so that we can just add it back when we want to.
                timedTouchSwitches = new Dictionary<Entity, Coroutine>();

                foreach (Entity entity in scene.Entities) {
                    if (entity.GetType().ToString() == "Celeste.Mod.OutbackHelper.TimedTouchSwitch") {
                        Coroutine coroutine = entity.Get<Coroutine>();
                        if (coroutine == null) {
                            // timed touch switch has no coroutine (possibly because it was already used on an earlier screen?), skip it.
                            continue;
                        }
                        entity.Remove(coroutine);
                        timedTouchSwitches.Add(entity, coroutine);
                    }
                }

                // share the map with all triggers in the room, because we want to process timed touch switches only once.
                foreach (ActivateTimedTouchSwitchesTimerTrigger trigger in scene.Tracker.GetEntities<ActivateTimedTouchSwitchesTimerTrigger>()) {
                    trigger.timedTouchSwitches = timedTouchSwitches;
                }
            }
        }

        public override void OnEnter(Player player) {
            foreach (KeyValuePair<Entity, Coroutine> pair in timedTouchSwitches) {
                // add the "time ticking" coroutine back
                pair.Key.Add(pair.Value);
            }

            // our work here is done... and so is the work of all other triggers in the room.
            foreach (ActivateTimedTouchSwitchesTimerTrigger trigger in Scene.Tracker.GetEntities<ActivateTimedTouchSwitchesTimerTrigger>()) {
                trigger.RemoveSelf();
            }
        }
    }
}
