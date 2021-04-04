using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/ActivateTimedTouchSwitchesTimerTrigger")]
    [Tracked]
    class ActivateTimedTouchSwitchesTimerTrigger : Trigger {
        private static FieldInfo timedTouchSwitchIcon;
        private static FieldInfo timedTouchSwitchStartColor;

        private Dictionary<Entity, Coroutine> timedTouchSwitches;

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

                        if (timedTouchSwitchIcon == null) {
                            // resolve reflections
                            timedTouchSwitchIcon = entity.GetType().GetField("icon", BindingFlags.NonPublic | BindingFlags.Instance);
                            timedTouchSwitchStartColor = entity.GetType().GetField("starterColor", BindingFlags.NonPublic | BindingFlags.Instance);
                        }

                        // make sure the touch switch color remains correct until the timer is activated.
                        entity.Add(new Coroutine(lockColorRoutine(entity)));
                    }
                }

                // share the map with all triggers in the room, because we want to process timed touch switches only once.
                foreach (ActivateTimedTouchSwitchesTimerTrigger trigger in scene.Tracker.GetEntities<ActivateTimedTouchSwitchesTimerTrigger>()) {
                    trigger.timedTouchSwitches = timedTouchSwitches;
                }
            }
        }

        private static IEnumerator lockColorRoutine(Entity timedTouchSwitch) {
            Sprite sprite = (Sprite) timedTouchSwitchIcon.GetValue(timedTouchSwitch);
            Color startColor = (Color) timedTouchSwitchStartColor.GetValue(timedTouchSwitch);
            while (!((TouchSwitch) timedTouchSwitch).Switch.Activated) {
                sprite.Color = startColor;
                yield return null;
            }
        }

        public override void OnEnter(Player player) {
            foreach (KeyValuePair<Entity, Coroutine> pair in timedTouchSwitches) {
                // remove lockColorRoutine
                pair.Key.Remove(pair.Key.Get<Coroutine>());

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
