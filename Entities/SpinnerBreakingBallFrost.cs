using FrostHelper;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /**
     * Spinner breaking ball that breaks Frost Helper spinners.
     */
    public class SpinnerBreakingBallFrost : SpinnerBreakingBallGeneric<CustomSpinner, Color> {
        public static new void Load() {
            SpinnerBreakingBallGeneric<CustomSpinner, Color>.Load();
            Everest.Events.Level.OnLoadEntity += onLoadEntity;
        }
        public static new void Unload() {
            SpinnerBreakingBallGeneric<CustomSpinner, Color>.Unload();
            Everest.Events.Level.OnLoadEntity -= onLoadEntity;
        }

        private static bool onLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            if (entityData.Name == "MaxHelpingHand/SpinnerBreakingBallFrostHelper") {
                level.Add(new SpinnerBreakingBallFrost(entityData, offset, new EntityID(levelData.Name, entityData.ID)));
                return true;
            }

            return false;
        }

        public SpinnerBreakingBallFrost(EntityData data, Vector2 offset, EntityID entityID)
            : base(data, offset, entityID, data.HexColor("color", Color.White)) {
        }

        protected override int getID(CustomSpinner spinner) {
            return spinner.ID;
        }

        protected override Color getColor(CustomSpinner spinner) {
            return spinner.Tint;
        }

        protected override bool getAttachToSolid(CustomSpinner spinner) {
            return spinner.AttachToSolid;
        }

        protected override void destroySpinner(CustomSpinner spinner) {
            spinner.Destroy();
        }
    }
}
