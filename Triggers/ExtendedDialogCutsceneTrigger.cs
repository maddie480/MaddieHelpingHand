using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    // Near copy-paste of Everest's Dialog Cutscene Trigger, that has the following extra features:
    // - allows to start auto-skipping messages (skipping them without pressing Confirm) with {trigger 0} and stop auto-skipping with {trigger 1}
    // - allows to set a custom font for the dialogue
    [CustomEntity("MaxHelpingHand/AutoSkipDialogCutsceneTrigger", "MaxHelpingHand/ExtendedDialogCutsceneTrigger")]
    public class ExtendedDialogCutsceneTrigger : Trigger {
        private readonly string dialogEntry;
        private readonly EntityID id;
        private readonly bool onlyOnce;
        private readonly bool endLevel;
        private readonly int deathCount;
        private readonly string font;

        private bool triggered;

        public ExtendedDialogCutsceneTrigger(EntityData data, Vector2 offset, EntityID entId)
            : base(data, offset) {
            dialogEntry = data.Attr("dialogId");
            onlyOnce = data.Bool("onlyOnce", true);
            endLevel = data.Bool("endLevel", false);
            deathCount = data.Int("deathCount", -1);
            font = data.Attr("font");
            triggered = false;
            id = entId;
        }

        public override void OnEnter(Player player) {
            if (triggered || (Scene as Level).Session.GetFlag("DoNotLoad" + id) ||
                (deathCount >= 0 && SceneAs<Level>().Session.DeathsInCurrentLevel != deathCount)) {

                return;
            }

            triggered = true;

            Scene.Add(new ExtendedDialogCutscene(dialogEntry, player, endLevel, font));

            if (onlyOnce) {
                (Scene as Level).Session.SetFlag("DoNotLoad" + id, true); // Sets flag to not load
            }
        }

        private class ExtendedDialogCutscene : CutsceneEntity {
            private Player player;
            private string dialogID;
            private bool endLevel;
            private string font;

            public ExtendedDialogCutscene(string dialogID, Player player, bool endLevel, string font) {
                this.dialogID = dialogID;
                this.player = player;
                this.endLevel = endLevel;
                this.font = font;
            }

            public override void OnBegin(Level level) {
                Add(new Coroutine(Cutscene(level)));
            }

            private IEnumerator Cutscene(Level level) {
                player.StateMachine.State = 11;
                player.StateMachine.Locked = true;
                player.ForceCameraUpdate = true;
                yield return ReplaceFancyTextFontFor(Textbox.Say(dialogID, startSkipping, stopSkipping), font);
                EndCutscene(level);
            }

            // this is triggered with {trigger 0}
            private IEnumerator startSkipping() {
                Scene.Tracker.GetEntity<Textbox>().autoPressContinue = !MaxHelpingHandModule.Instance.Settings.DisableDialogueAutoSkip;
                yield break;
            }

            // this is triggered with {trigger 1}
            private IEnumerator stopSkipping() {
                Scene.Tracker.GetEntity<Textbox>().autoPressContinue = false;
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

        internal static IEnumerator ReplaceFancyTextFontFor(IEnumerator coroutine, string font) {
            if (!string.IsNullOrEmpty(font)) {
                ILContext.Manipulator hook = replaceFontInFancyTextConstructor(font);

                if (Fonts.Get(font) == null) {
                    // this is a font we need to load for the cutscene specifically!
                    if (!Fonts.paths.ContainsKey(font)) {
                        // the font isn't in the list... so we need to list fonts again first.
                        Logger.Log(LogLevel.Warn, "MaxHelpingHand/ExtendedDialogCutsceneTrigger", $"We need to list fonts again, {font} does not exist!");
                        Fonts.Prepare();
                    }

                    Fonts.Load(font);
                    Engine.Scene.Add(new FontHolderEntity(font));
                }

                // the first step of the coroutine deals with the font setup, so we need to replace the font at that precise moment.
                IL.Celeste.FancyText.ctor += hook;
                coroutine.MoveNext();
                IL.Celeste.FancyText.ctor -= hook;

                // yield return whatever we consumed in the previous MoveNext.
                yield return coroutine.Current;
            }

            // consume the rest of the coroutine.
            yield return new SwapImmediately(coroutine);
        }

        private static ILContext.Manipulator replaceFontInFancyTextConstructor(string fontName) {
            return il => {
                ILCursor cursor = new ILCursor(il);
                while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Language>("FontFace"))) {
                    Logger.Log("MaxHelpingHand/ExtendedDialogCutsceneTrigger", $"Replacing font at {cursor.Index} in IL for FancyText constructor");
                    cursor.Emit(OpCodes.Pop);
                    cursor.Emit(OpCodes.Ldstr, fontName);
                }
            };
        }

        // a small entity that just ensures a font loaded by a cutscene doesn't stay loaded forever:
        // it unloads the font whenever you leave the room or return to map (even during the cutscene).
        private class FontHolderEntity : Entity {
            private string font;

            public FontHolderEntity(string font) {
                this.font = font;
            }

            public override void Removed(Scene scene) {
                base.Removed(scene);
                Fonts.Unload(font);
            }

            public override void SceneEnd(Scene scene) {
                base.SceneEnd(scene);
                Fonts.Unload(font);
            }
        }
    }
}
