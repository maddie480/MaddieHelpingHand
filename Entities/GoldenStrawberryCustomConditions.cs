using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/GoldenStrawberryCustomConditions")]
    [RegisterStrawberry(tracked: false, blocksCollection: true)]
    static class GoldenStrawberryCustomConditions {
        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            EntityID entityID = new EntityID(levelData.Name, entityData.ID);

            bool notDiedOrVisitedFurtherRooms = level.Session.FurthestSeenLevel == level.Session.Level || level.Session.Deaths == 0;
            bool unlockedCSides = SaveData.Instance.UnlockedModes >= 3 || SaveData.Instance.DebugMode;
            bool completed = SaveData.Instance.Areas_Safe[level.Session.Area.ID].Modes[(int) level.Session.Area.Mode].Completed;

            if (!entityData.Bool("mustNotDieAndVisitFurtherRooms", true)) {
                notDiedOrVisitedFurtherRooms = true;
            }
            if (!entityData.Bool("mustHaveUnlockedCSides", true)) {
                unlockedCSides = true;
            }
            if (!entityData.Bool("mustHaveCompletedLevel", true)) {
                completed = true;
            }

            if ((SaveData.Instance.CheatMode || (unlockedCSides && completed)) && notDiedOrVisitedFurtherRooms) {
                Strawberry berry = new Strawberry(entityData, offset, entityID);
                new DynData<Strawberry>(berry)["Golden"] = true;
                return berry;
            }
            return null;
        }
    }
}
