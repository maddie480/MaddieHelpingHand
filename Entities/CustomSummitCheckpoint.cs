using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomSummitCheckpoint = GenerateCustomSummitCheckpoint")]
    public class CustomSummitCheckpoint : SummitCheckpoint {
        private static FieldInfo confettiColorsFieldInfo = typeof(ConfettiRenderer).GetField("confettiColors", BindingFlags.NonPublic | BindingFlags.Static);

        public static Entity GenerateCustomSummitCheckpoint(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            // internally, the number will be the entity ID to ensure all "summit_checkpoint_{number}" session flags are unique.
            // we also add 100 to be sure not to conflict with vanilla checkpoints.
            entityData.Values["number"] = entityData.ID + 100;
            return new CustomSummitCheckpoint(entityData, offset);
        }

        private readonly Color[] confettiColors;

        public CustomSummitCheckpoint(EntityData data, Vector2 offset) : base(data, offset) {
            confettiColors = parseColors(data.Attr("confettiColors", "fe2074,205efe,cefe20"));

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

        public override void Update() {
            if (!Activated && CollideCheck<Player>()) {
                // player is potentially triggering the checkpoint => change the confetti colors!
                Color[] vanillaConfetti = (Color[]) confettiColorsFieldInfo.GetValue(null);
                confettiColorsFieldInfo.SetValue(null, confettiColors);

                base.Update();

                confettiColorsFieldInfo.SetValue(null, vanillaConfetti);
            } else {
                // checkpoint can't be triggered, so don't mess with confetti to avoid useless reflection.
                base.Update();
            }
        }
    }
}
