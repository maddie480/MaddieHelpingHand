using Celeste.Mod.MaxHelpingHand.Effects;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.MaxHelpingHand.Triggers;
using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandModule : EverestModule {
        public static MaxHelpingHandModule Instance { get; private set; }

        public override Type SettingsType => typeof(MaxHelpingHandSettings);
        public MaxHelpingHandSettings Settings => (MaxHelpingHandSettings) _Settings;

        public override Type SessionType => typeof(MaxHelpingHandSession);
        public MaxHelpingHandSession Session => (MaxHelpingHandSession) _Session;

        public MaxHelpingHandModule() {
            Instance = this;
        }

        public override void Load() {
            Logger.SetLogLevel("MaxHelpingHand", LogLevel.Info);

            TempleEyeTrackingMadeline.Load();
            CameraCatchupSpeedTrigger.Load();
            FlagTouchSwitch.Load();
            UpsideDownJumpThru.Load();
            SidewaysJumpThru.Load();
            GroupedDustTriggerSpikes.Load();
            StaticPuffer.Load();
            BlackholeCustomColors.Load();
            ColorGradeFadeTrigger.Load();
            RainbowSpinnerColorController.Load();
            ReskinnableSwapBlock.Load();
            ReskinnableCrushBlock.Load();
            KevinBarrier.Load();
            GradientDustTrigger.Load();
            GoldenStrawberryCustomConditions.Load();
            MadelineSilhouetteTrigger.Load();
            BumperNotCoreMode.Load();
            MultiRoomStrawberrySeed.Load();
            SpeedBasedMusicParamTrigger.Load();
            AnimatedParallax.Load();
            NorthernLightsCustomColors.Load();
            ReskinnableFloatingDebris.Load();
            GuiStrawberryReskin.Load();
            SeekerBarrierColorController.Load();
            AmbienceVolumeTrigger.Load();
            CustomTutorialWithNoBird.Load();
            NonPoppingStrawberrySeed.Load();
            CustomizableCrumblePlatform.Load();
            MovingFlagTouchSwitch.Load();
            HintDecal.Load();
            CameraOffsetBorder.Load();
            DisableIcePhysicsTrigger.Load();
            ReskinnableBillboard.Load();
            OneWayCameraTrigger.Load();

            Everest.Events.Level.OnLoadBackdrop += onLoadBackdrop;
        }

        public override void Unload() {
            TempleEyeTrackingMadeline.Unload();
            CameraCatchupSpeedTrigger.Unload();
            FlagTouchSwitch.Unload();
            UpsideDownJumpThru.Unload();
            SidewaysJumpThru.Unload();
            GroupedDustTriggerSpikes.Unload();
            StaticPuffer.Unload();
            BlackholeCustomColors.Unload();
            ColorGradeFadeTrigger.Unload();
            RainbowSpinnerColorController.Unload();
            ReskinnableSwapBlock.Unload();
            ReskinnableCrushBlock.Unload();
            KevinBarrier.Unload();
            GradientDustTrigger.Unload();
            GoldenStrawberryCustomConditions.Unload();
            MadelineSilhouetteTrigger.Unload();
            BumperNotCoreMode.Unload();
            MultiRoomStrawberrySeed.Unload();
            SpeedBasedMusicParamTrigger.Unload();
            AnimatedParallax.Unload();
            NorthernLightsCustomColors.Unload();
            ReskinnableFloatingDebris.Unload();
            GuiStrawberryReskin.Unload();
            SeekerBarrierColorController.Unload();
            AmbienceVolumeTrigger.Unload();
            CustomTutorialWithNoBird.Unload();
            NonPoppingStrawberrySeed.Unload();
            CustomizableCrumblePlatform.Unload();
            MovingFlagTouchSwitch.Unload();
            HintDecal.Unload();
            CameraOffsetBorder.Unload();
            DisableIcePhysicsTrigger.Unload();
            ReskinnableBillboard.Unload();
            OneWayCameraTrigger.Unload();

            Everest.Events.Level.OnLoadBackdrop -= onLoadBackdrop;
        }

        private Backdrop onLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above) {
            if (child.Name.Equals("MaxHelpingHand/HeatWaveNoColorGrade", StringComparison.OrdinalIgnoreCase)) {
                return new HeatWaveNoColorGrade(child.AttrBool("controlColorGradeWhenActive"));
            }
            if (child.Name.Equals("MaxHelpingHand/BlackholeCustomColors", StringComparison.OrdinalIgnoreCase)) {
                return BlackholeCustomColors.CreateBlackholeWithCustomColors(child);
            }
            if (child.Name.Equals("MaxHelpingHand/CustomPlanets", StringComparison.OrdinalIgnoreCase)) {
                return new CustomPlanets(child.AttrInt("count", 32), child.Attr("directory", "MaxHelpingHand/customplanets/bigstars"), child.AttrFloat("animationDelay", 0.1f));
            }
            if (child.Name.Equals("MaxHelpingHand/CustomStars", StringComparison.OrdinalIgnoreCase)) {
                int? starCount = null;
                if (int.TryParse(child.Attr("starCount", ""), out int starCountParsed)) {
                    starCount = starCountParsed;
                }
                string tint = child.Attr("tint", "");
                if (child.AttrBool("disableTinting", false)) {
                    tint = "ffffff"; // approximative backwards compatibility
                }
                return new CustomStars(starCount, string.IsNullOrEmpty(tint) ? (Color?) null : Calc.HexToColor(tint), child.Attr("spriteDirectory", "bgs/02/stars"));
            }
            if (child.Name.Equals("MaxHelpingHand/SnowCustomColors", StringComparison.OrdinalIgnoreCase)) {
                string[] colorsAsStrings = child.Attr("colors").Split(',');
                Color[] colors = new Color[colorsAsStrings.Length];
                for (int i = 0; i < colors.Length; i++) {
                    colors[i] = Calc.HexToColor(colorsAsStrings[i]);
                }

                return new SnowCustomColors(colors, child.AttrBool("foreground"));
            }
            if (child.Name.Equals("MaxHelpingHand/NorthernLightsCustomColors", StringComparison.OrdinalIgnoreCase)) {
                string[] colorsAsStrings = child.Attr("colors").Split(',');
                Color[] colors = new Color[colorsAsStrings.Length];
                for (int i = 0; i < colors.Length; i++) {
                    colors[i] = Calc.HexToColor(colorsAsStrings[i]);
                }

                NorthernLightsCustomColors.GradientColor1 = child.Attr("gradientColor1", "020825");
                NorthernLightsCustomColors.GradientColor2 = child.Attr("gradientColor2", "170c2f");
                NorthernLightsCustomColors.Colors = colors;

                NorthernLightsCustomColors effect = new NorthernLightsCustomColors(colors, child.AttrBool("displayBackground", true));

                NorthernLightsCustomColors.GradientColor1 = null;
                NorthernLightsCustomColors.GradientColor2 = null;
                NorthernLightsCustomColors.Colors = null;

                return effect;
            }
            return null;
        }

        public override void PrepareMapDataProcessors(MapDataFixup context) {
            base.PrepareMapDataProcessors(context);

            context.Add<MaxHelpingHandMapDataProcessor>();
        }
    }
}
