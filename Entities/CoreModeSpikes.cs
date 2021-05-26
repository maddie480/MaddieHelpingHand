using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity(
        "MaxHelpingHand/CoreModeSpikesUp = LoadUp",
        "MaxHelpingHand/CoreModeSpikesDown = LoadDown",
        "MaxHelpingHand/CoreModeSpikesLeft = LoadLeft",
        "MaxHelpingHand/CoreModeSpikesRight = LoadRight"
    )]
    [TrackedAs(typeof(Spikes))]
    public class CoreModeSpikes : Spikes {
        public static Entity LoadUp(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            entityData.Values["type"] = entityData.Attr("hotType", "default");
            return new CoreModeSpikes(entityData, offset, Directions.Up);
        }
        public static Entity LoadDown(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            entityData.Values["type"] = entityData.Attr("hotType", "default");
            return new CoreModeSpikes(entityData, offset, Directions.Down);
        }
        public static Entity LoadLeft(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            entityData.Values["type"] = entityData.Attr("hotType", "default");
            return new CoreModeSpikes(entityData, offset, Directions.Left);
        }
        public static Entity LoadRight(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            entityData.Values["type"] = entityData.Attr("hotType", "default");
            return new CoreModeSpikes(entityData, offset, Directions.Right);
        }

        private string hotType;
        private string coldType;

        private int randomSeed;

        public CoreModeSpikes(EntityData data, Vector2 offset, Directions dir) : base(data, offset, dir) {
            hotType = data.Attr("hotType", "default");
            coldType = data.Attr("coldType", "default");

            Add(new CoreModeListener(onCoreModeChange));
            randomSeed = Calc.Random.Next();
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            swapTexture(SceneAs<Level>().Session.CoreMode);
        }

        private void onCoreModeChange(Session.CoreModes newCoreMode) {
            swapTexture(newCoreMode);
        }

        private void swapTexture(Session.CoreModes target) {
            string spikeType = target == Session.CoreModes.Cold ? coldType : hotType;
            string direction = Direction.ToString().ToLower();
            List<MTexture> spikeTextures = GFX.Game.GetAtlasSubtextures("danger/spikes/" + spikeType + "_" + direction);

            // be sure to use the same seed to pick the same spike indices when switching.
            Calc.PushRandom(randomSeed);
            foreach (Image image in Components.GetAll<Image>()) {
                image.Texture = Calc.Random.Choose(spikeTextures);
            }
            Calc.PopRandom();
        }
    }
}
