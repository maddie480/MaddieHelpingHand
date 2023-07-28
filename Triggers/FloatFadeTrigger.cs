using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity(
        "MaxHelpingHand/BloomBaseFadeTrigger = LoadBloomBaseFadeTrigger",
        "MaxHelpingHand/BloomStrengthFadeTrigger = LoadBloomStrengthFadeTrigger",
        "MaxHelpingHand/DarknessAlphaFadeTrigger = LoadDarknessAlphaFadeTrigger"
    )]
    public class FloatFadeTrigger : Trigger {
        public static Entity LoadBloomBaseFadeTrigger(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            return new FloatFadeTrigger(entityData, offset, value => {
                level.Bloom.Base = value;
                level.Session.BloomBaseAdd = value - AreaData.Get(level).BloomBase;
            });
        }

        public static Entity LoadBloomStrengthFadeTrigger(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            return new FloatFadeTrigger(entityData, offset, value => {
                level.Bloom.Strength = value;
                MaxHelpingHandModule.Instance.Session.BloomStregth = value;
            });
        }

        public static Entity LoadDarknessAlphaFadeTrigger(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            return new FloatFadeTrigger(entityData, offset, value => {
                level.Lighting.Alpha = value;
                level.Session.LightingAlphaAdd = value - level.BaseLightingAlpha;
            });
        }


        private readonly float fadeA;
        private readonly float fadeB;
        private readonly PositionModes positionMode;
        private readonly Action<float> applyFadeAction;

        public FloatFadeTrigger(EntityData data, Vector2 offset, Action<float> applyFadeAction) : base(data, offset) {
            fadeA = data.Float("fadeA");
            fadeB = data.Float("fadeB");
            positionMode = data.Enum<PositionModes>("positionMode");
            this.applyFadeAction = applyFadeAction;
        }

        public override void OnStay(Player player) {
            applyFadeAction(Calc.ClampedMap(GetPositionLerp(player, positionMode), 0f, 1f, fadeA, fadeB));
        }
    }
}
