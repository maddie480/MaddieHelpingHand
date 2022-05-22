using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomSummitCheckpoint = GenerateCustomSummitCheckpoint")]
    [Tracked]
    public class CustomSummitCheckpoint : SummitCheckpoint {
        private static FieldInfo confettiColorsFieldInfo = typeof(ConfettiRenderer).GetField("confettiColors", BindingFlags.NonPublic | BindingFlags.Static);

        public static Entity GenerateCustomSummitCheckpoint(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            // internally, the number will be the entity ID to ensure all "summit_checkpoint_{number}" session flags are unique.
            // we also add 100 to be sure not to conflict with vanilla checkpoints.
            entityData.Values["number"] = entityData.ID + 100;
            return new CustomSummitCheckpoint(entityData, offset);
        }

        private readonly Color[] confettiColors;
        private readonly string groupFlag;

        public CustomSummitCheckpoint(EntityData data, Vector2 offset) : base(data, offset) {
            confettiColors = parseColors(data.Attr("confettiColors", "fe2074,205efe,cefe20"));
            groupFlag = data.Attr("groupFlag");

            DynData<SummitCheckpoint> self = new DynData<SummitCheckpoint>(this);

            string firstDigit = data.Attr("firstDigit");
            string secondDigit = data.Attr("secondDigit");

            string directory = data.Attr("spriteDirectory", defaultValue: "MaxHelpingHand/summitcheckpoints");

            // reshuffle the loaded textures: the first wanted digit at index 0, the second one at index 1.
            // so, the string to display is always "01" no matter what
            self["numberString"] = "01";
            self["numbersEmpty"] = new List<MTexture>() {
                GFX.Game[$"{directory}/{firstDigit}/numberbg"],
                GFX.Game[$"{directory}/{secondDigit}/numberbg"]
            };
            self["numbersActive"] = new List<MTexture>() {
                GFX.Game[$"{directory}/{firstDigit}/number"],
                GFX.Game[$"{directory}/{secondDigit}/number"]
            };

            // customize the background textures.
            self["baseEmpty"] = GFX.Game[$"{directory}/base00"];
            self["baseToggle"] = GFX.Game[$"{directory}/base01"];
            self["baseActive"] = GFX.Game[$"{directory}/base02"];
        }

        private static Color[] parseColors(string input) {
            string[] colorsAsStrings = input.Split(',');
            Color[] colors = new Color[colorsAsStrings.Length];
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = Calc.HexToColor(colorsAsStrings[i]);
            }
            return colors;
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // if another checkpoint in the group was activated, we should activate this checkpoint.
            if (!string.IsNullOrEmpty(groupFlag) && (scene as Level).Session.GetFlag(groupFlag)) {
                Activated = true;
            }
        }

        public override void Awake(Scene scene) {
            bool wasActivated = Activated;

            base.Awake(scene);

            if (!wasActivated && Activated && !string.IsNullOrEmpty(groupFlag)) {
                // summit checkpoint was enabled due to spawning on it, enable the rest of the group without any effects.
                triggerGroupFlag(otherCheckpoint => otherCheckpoint.Activated = true);
            }
        }

        public override void Update() {
            bool wasActivated = Activated;

            if (!Activated && CollideCheck<Player>()) {
                // player is potentially triggering the checkpoint => change the confetti colors!
                runWithModdedConfetti(confettiColors, () => base.Update());
            } else {
                // checkpoint can't be triggered, so don't mess with confetti to avoid useless reflection.
                base.Update();
            }

            if (!wasActivated && Activated && !string.IsNullOrEmpty(groupFlag)) {
                // enable entire group in a similar way, with the confetti effect.
                triggerGroupFlag(otherCheckpoint => {
                    otherCheckpoint.Activated = true;
                    otherCheckpoint.SceneAs<Level>().Displacement.AddBurst(otherCheckpoint.Position, 0.5f, 4f, 24f, 0.5f);

                    runWithModdedConfetti(otherCheckpoint.confettiColors, () => {
                        otherCheckpoint.SceneAs<Level>().Add(new ConfettiRenderer(otherCheckpoint.Position));
                    });

                    Audio.Play("event:/game/07_summit/checkpoint_confetti", otherCheckpoint.Position);
                });
            }
        }

        private static void runWithModdedConfetti(Color[] confettiColors, Action toRun) {
            Color[] vanillaConfetti = (Color[]) confettiColorsFieldInfo.GetValue(null);
            confettiColorsFieldInfo.SetValue(null, confettiColors);

            toRun();

            confettiColorsFieldInfo.SetValue(null, vanillaConfetti);
        }

        private void triggerGroupFlag(Action<CustomSummitCheckpoint> actionOnEntireGroup) {
            // if this checkpoint was just activated, it should set the group flag...
            SceneAs<Level>().Session.SetFlag(groupFlag);

            // and look for other summit checkpoints that have the same group flag, in order to activate them.
            foreach (CustomSummitCheckpoint other in Scene.Tracker.GetEntities<CustomSummitCheckpoint>()) {
                if (!other.Activated && other.groupFlag == groupFlag) {
                    actionOnEntireGroup(other);
                }
            }
        }
    }
}
