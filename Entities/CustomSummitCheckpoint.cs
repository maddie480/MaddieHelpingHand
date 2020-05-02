using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomSummitCheckpoint = GenerateCustomSummitCheckpoint")]
    class CustomSummitCheckpoint : SummitCheckpoint {
        public static Entity GenerateCustomSummitCheckpoint(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            // internally, the number will be the entity ID to ensure all "summit_checkpoint_{number}" session flags are unique.
            // we also add 100 to be sure not to conflict with vanilla checkpoints.
            entityData.Values["number"] = entityData.ID + 100;
            return new CustomSummitCheckpoint(entityData, offset);
        }

        public CustomSummitCheckpoint(EntityData data, Vector2 offset) : base(data, offset) {
            DynData<SummitCheckpoint> self = new DynData<SummitCheckpoint>(this);

            string firstDigit = data.Attr("firstDigit");
            string secondDigit = data.Attr("secondDigit");

            // reshuffle the loaded textures: the first wanted digit at index 0, the second one at index 1.
            // so, the string to display is always "01" no matter what
            self["numberString"] = "01";
            self["numbersEmpty"] = new List<MTexture>() {
                GFX.Game[$"MaxHelpingHand/summitcheckpoints/{firstDigit}/numberbg"],
                GFX.Game[$"MaxHelpingHand/summitcheckpoints/{secondDigit}/numberbg"]
            };
            self["numbersActive"] = new List<MTexture>() {
                GFX.Game[$"MaxHelpingHand/summitcheckpoints/{firstDigit}/number"],
                GFX.Game[$"MaxHelpingHand/summitcheckpoints/{secondDigit}/number"]
            };
        }
    }
}
