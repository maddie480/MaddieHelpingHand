using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    // Near copy-paste of Everest's Dialog Cutscene Trigger, that allows to start auto-skipping messages
    // (skipping them without pressing Confirm) with {trigger 0} and stop auto-skipping with {trigger 1}.
    [CustomEntity("MaxHelpingHand/AutoSkipDialogCutsceneTrigger")]
    public class AutoSkipDialogCutsceneTrigger : Trigger {
        private readonly string dialogEntry;
        private readonly EntityID id;
        private readonly bool onlyOnce;
        private readonly bool endLevel;
        private readonly int deathCount;

        private bool triggered;

        public AutoSkipDialogCutsceneTrigger(EntityData data, Vector2 offset, EntityID entId)
            : base(data, offset) {
            dialogEntry = data.Attr("dialogId");
            onlyOnce = data.Bool("onlyOnce", true);
            endLevel = data.Bool("endLevel", false);
            deathCount = data.Int("deathCount", -1);
            triggered = false;
            id = entId;
        }

        public override void OnEnter(Player player) {
            if (triggered || (Scene as Level).Session.GetFlag("DoNotLoad" + id) ||
                (deathCount >= 0 && SceneAs<Level>().Session.DeathsInCurrentLevel != deathCount)) {

                return;
            }

            triggered = true;

            Scene.Add(new AutoSkipDialogCutscene(dialogEntry, player, endLevel));

            if (onlyOnce) {
                (Scene as Level).Session.SetFlag("DoNotLoad" + id, true); // Sets flag to not load
            }
        }

        private class AutoSkipDialogCutscene : CutsceneEntity {
            private Player player;
            private string dialogID;
            private bool endLevel;

            public AutoSkipDialogCutscene(string dialogID, Player player, bool endLevel) {
                this.dialogID = dialogID;
                this.player = player;
                this.endLevel = endLevel;
            }

            public override void OnBegin(Level level) {
                Add(new Coroutine(Cutscene(level)));
            }

            private IEnumerator Cutscene(Level level) {
                player.StateMachine.State = 11;
                player.StateMachine.Locked = true;
                player.ForceCameraUpdate = true;
                yield return Textbox.Say(dialogID, startSkipping, stopSkipping);
                EndCutscene(level);
            }

            // this is triggered with {trigger 0}
            private IEnumerator startSkipping() {
                new DynData<Textbox>(Scene.Tracker.GetEntity<Textbox>())["autoPressContinue"] = !MaxHelpingHandModule.Instance.Settings.DisableDialogueAutoSkip;
                yield break;
            }

            // this is triggered with {trigger 1}
            private IEnumerator stopSkipping() {
                new DynData<Textbox>(Scene.Tracker.GetEntity<Textbox>())["autoPressContinue"] = false;
                yield break;
            }

            public override void OnEnd(Level level) {
                player.StateMachine.Locked = false;
                player.StateMachine.State = 0;
                player.ForceCameraUpdate = false;
                if (WasSkipped) {
                    level.Camera.Position = player.CameraTarget;
                }
                if (endLevel) {
                    level.CompleteArea(true, false);
                    player.StateMachine.State = 11;
                    RemoveSelf();
                }
            }
        }
    }
}
