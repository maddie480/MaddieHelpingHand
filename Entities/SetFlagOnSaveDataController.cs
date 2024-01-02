using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity(
        "MaxHelpingHand/SetFlagOnHeartCollectedController = LoadHeart",
        "MaxHelpingHand/SetFlagOnCompletionController = LoadCompletion",
        "MaxHelpingHand/SetFlagOnFullClearController = LoadFullClear"
    )]
    public class SetFlagOnSaveDataController : Entity {
        public static Entity LoadHeart(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            return new SetFlagOnSaveDataController(modeStats => modeStats.HeartGem, entityData.Attr("flag"));
        }
        public static Entity LoadCompletion(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            return new SetFlagOnSaveDataController(modeStats => modeStats.Completed, entityData.Attr("flag"));
        }
        public static Entity LoadFullClear(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            return new SetFlagOnSaveDataController(modeStats => modeStats.FullClear, entityData.Attr("flag"));
        }


        private readonly Func<AreaModeStats, bool> condition;
        private readonly string flag;
        private AreaModeStats modeStats;

        public SetFlagOnSaveDataController(Func<AreaModeStats, bool> condition, string flag) {
            this.condition = condition;
            this.flag = flag;
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            AreaKey areaKey = SceneAs<Level>().Session.Area;
            modeStats = SaveData.Instance.GetAreaStatsFor(areaKey).Modes[(int) areaKey.Mode];
            SceneAs<Level>().Session.SetFlag(flag, condition(modeStats));
        }

        public override void Update() {
            base.Update();

            SceneAs<Level>().Session.SetFlag(flag, condition(modeStats));
        }
    }
}
