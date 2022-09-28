using Celeste.Mod.MaxHelpingHand.Effects;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.MaxHelpingHand.Triggers;
using Monocle;
using Microsoft.Xna.Framework;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandModule : EverestModule {
        private bool hookedSineParallax = false;

        private static FieldInfo contentLoaded = typeof(Everest).GetField("_ContentLoaded", BindingFlags.NonPublic | BindingFlags.Static);

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
            HintDecal.Load();
            CameraOffsetBorder.Load();
            DisableIcePhysicsTrigger.Load();
            ReskinnableBillboard.Load();
            OneWayCameraTrigger.Load();
            MadelinePonytailTrigger.Load();
            SecretBerry.Load();
            CustomizableGlassBlockController.Load();
            CustomWipe.Load();
            AllSideTentacles.Load();
            SetFlagOnSpawnController.Load();
            NoDashRefillSpring.Load();
            HdParallax.Load();
            HeatWaveNoColorGrade.Load();
            ActivateTimedTouchSwitchesTimerTrigger.Load();
            SetBloomStrengthTrigger.Load();
            InstantLavaBlockerTrigger.Load();
            MadelineSprite.Load();
            StaticMoverWithLiftSpeed.Load();
            SpinnerBreakingBall.Load();
            ReskinnableCrystalHeart.Load();
            SetFlagOnButtonPressController.Load();
            FlagPickup.Load();
            RespawningJellyfish.Load();
            SetFlagOnSpawnTrigger.Load();

            Everest.Events.Level.OnLoadBackdrop += onLoadBackdrop;
            On.Celeste.Mod.Everest.Register += onModRegister;
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
            MadelinePonytailTrigger.Unload();
            SecretBerry.Unload();
            CustomizableGlassBlockController.Unload();
            CustomWipe.Unload();
            AllSideTentacles.Unload();
            SetFlagOnSpawnController.Unload();
            NoDashRefillSpring.Unload();
            HdParallax.Unload();
            HeatWaveNoColorGrade.Unload();
            ActivateTimedTouchSwitchesTimerTrigger.Unload();
            SetBloomStrengthTrigger.Unload();
            InstantLavaBlockerTrigger.Unload();
            MadelineSprite.Unload();
            StaticMoverWithLiftSpeed.Unload();
            SpinnerBreakingBall.Unload();
            ReskinnableCrystalHeart.Unload();
            SetFlagOnButtonPressController.Unload();
            FlagPickup.Unload();
            AvBdaySpeechBubbleFixup.Unload();
            RespawningJellyfish.Unload();
            ReskinnableStarRotateSpinner.Unload();
            ReskinnableStarTrackSpinner.Unload();
            SetFlagOnSpawnTrigger.Unload();

            Everest.Events.Level.OnLoadBackdrop -= onLoadBackdrop;
            On.Celeste.Mod.Everest.Register -= onModRegister;

            if (hookedSineParallax) {
                unhookSineParallax();
            }
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);

            MultiRoomStrawberryCounter.Initialize();
            HookMods();
        }

        private void onModRegister(On.Celeste.Mod.Everest.orig_Register orig, EverestModule module) {
            orig(module);

            if ((bool) contentLoaded.GetValue(null)) {
                // the game was already initialized and a new mod was loaded at runtime:
                // make sure whe applied all mod hooks we want to apply.
                HookMods();
            }
        }

        private void HookMods() {
            KevinBarrier.HookMods();
            MovingFlagTouchSwitch.HookMods();
            MadelinePonytailTrigger.LoadContent();
            InstantLavaBlockerTrigger.HookMods();
            MadelineSprite.HookMods();
            ReskinnableCrystalHeart.HookMods();
            AvBdaySpeechBubbleFixup.LoadMods();
            ReskinnableStarRotateSpinner.LoadMods();
            ReskinnableStarTrackSpinner.LoadMods();

            if (!hookedSineParallax && Everest.Loader.DependencyLoaded(new EverestModuleMetadata() { Name = "FlaglinesAndSuch", Version = new Version(1, 4, 17) })) {
                hookSineParallax();
            }
        }

        private void hookSineParallax() {
            SineAnimatedParallax.Load();
            hookedSineParallax = true;
        }

        private void unhookSineParallax() {
            SineAnimatedParallax.Unload();
            hookedSineParallax = false;
        }

        private Backdrop onLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above) {
            if (child.Name.Equals("MaxHelpingHand/HeatWaveNoColorGrade", StringComparison.OrdinalIgnoreCase)) {
                return new HeatWaveNoColorGrade(child.AttrBool("controlColorGradeWhenActive"), child.AttrBool("renderParticles", defaultValue: true));
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
                // parse the alpha field if it's present and not empty. Otherwise, pass null to CustomStars
                float? alpha = null;
                string alphaString = child.Attr("starAlpha");
                if (!string.IsNullOrEmpty(alphaString)) {
                    alpha = float.Parse(alphaString);
                }
                return new CustomStars(starCount, string.IsNullOrEmpty(tint) ? (Color?) null : Calc.HexToColor(tint), child.Attr("spriteDirectory", "bgs/02/stars"),
                    child.AttrFloat("wrapHeight", 180f), alpha, child.AttrFloat("bgAlpha", 1f));
            }
            if (child.Name.Equals("MaxHelpingHand/CustomStarfield", StringComparison.OrdinalIgnoreCase)) {
                string[] paths = child.Attr("paths", "starfield").Split(',');

                string[] colorsAsStrings = child.Attr("colors", "ffffff").Split(',');
                Color[] colors = new Color[colorsAsStrings.Length];
                for (int i = 0; i < colorsAsStrings.Length; i++) {
                    colors[i] = Calc.HexToColor(colorsAsStrings[i]);
                }

                string[] alphasAsStrings = child.Attr("alphas", "1").Split(',');
                float[] alphas = new float[alphasAsStrings.Length];
                for (int i = 0; i < alphasAsStrings.Length; i++) {
                    alphas[i] = float.Parse(alphasAsStrings[i]);
                }

                return new CustomStarfield(paths, colors, alphas, child.AttrBool("shuffle", true), child.AttrFloat("speed", 1f));
            }
            if (child.Name.Equals("MaxHelpingHand/SnowCustomColors", StringComparison.OrdinalIgnoreCase)) {
                string[] colorsAsStrings = child.Attr("colors").Split(',');
                Color[] colors = new Color[colorsAsStrings.Length];
                for (int i = 0; i < colors.Length; i++) {
                    colors[i] = Calc.HexToColor(colorsAsStrings[i]);
                }

                return new SnowCustomColors(colors, child.AttrFloat("speedMin", 40f), child.AttrFloat("speedMax", 100f));
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
                NorthernLightsCustomColors.ParticleCount = child.AttrInt("particleCount", 50);
                NorthernLightsCustomColors.StrandCount = child.AttrInt("strandCount", 3);

                NorthernLightsCustomColors effect = new NorthernLightsCustomColors(colors, child.AttrBool("displayBackground", true));

                NorthernLightsCustomColors.GradientColor1 = null;
                NorthernLightsCustomColors.GradientColor2 = null;
                NorthernLightsCustomColors.Colors = null;
                NorthernLightsCustomColors.ParticleCount = 0;
                NorthernLightsCustomColors.StrandCount = 0;

                return effect;
            }
            if (child.Name.Equals("MaxHelpingHand/AllSideTentacles", StringComparison.OrdinalIgnoreCase)) {
                return new AllSideTentacles((Tentacles.Side) Enum.Parse(typeof(Tentacles.Side), child.Attr("side", "Right")), Calc.HexToColor(child.Attr("color")), child.AttrFloat("offset"));
            }
            return null;
        }

        public override void PrepareMapDataProcessors(MapDataFixup context) {
            base.PrepareMapDataProcessors(context);

            context.Add<MaxHelpingHandMapDataProcessor>();
        }
    }
}
