using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/GoldenStrawberryCustomConditions")]
    [RegisterStrawberry(tracked: false, blocksCollection: true)]
    public static class GoldenStrawberryCustomConditions {
        public static void Load() {
            On.Celeste.MapData.Load += modMapDataLoad;
        }

        public static void Unload() {
            On.Celeste.MapData.Load -= modMapDataLoad;
        }

        private static void modMapDataLoad(On.Celeste.MapData.orig_Load orig, MapData self) {
            orig(self);

            // if the option is enabled, register golden berries with custom conditions as golden berries.
            foreach (LevelData level in self.Levels) {
                foreach (EntityData entity in level.Entities) {
                    if (entity.Name == "MaxHelpingHand/GoldenStrawberryCustomConditions" && entity.Bool("showGoldenChapterCard", true)) {
                        self.Goldenberries.Add(entity);
                    }
                }
            }
        }


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
                berry.Golden = true;
                return berry;
            }
            return null;
        }
    }
}
