using Celeste.Mod.ExCameraDynamics.Code.Module;
using Celeste.Mod.MaxHelpingHand.Effects;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.MaxHelpingHand.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.ModInterop;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandModule : EverestModule {
        private bool hookedSineParallax = false;
        private bool bounceHelperLoaded = false;
        private bool frostBreakingBallLoaded = false;

        private static FieldInfo contentLoaded = typeof(Everest).GetField("_ContentLoaded", BindingFlags.NonPublic | BindingFlags.Static);

        public static MaxHelpingHandModule Instance { get; private set; }

        public override Type SettingsType => typeof(MaxHelpingHandSettings);
        public MaxHelpingHandSettings Settings => (MaxHelpingHandSettings) _Settings;

        public override Type SaveDataType => typeof(MaxHelpingHandSaveData);
        public MaxHelpingHandSaveData SaveData => (MaxHelpingHandSaveData) _SaveData;

        public override Type SessionType => typeof(MaxHelpingHandSession);
        public MaxHelpingHandSession Session => (MaxHelpingHandSession) _Session;

        private static bool extendedCameraDynamicsEnabled = false;
        private bool extendedCameraDynamicsHookEnabled = false;

        private static bool zoomOutHelperPrototypeEnabled = false;
        private bool zoomOutHelperPrototypeHookEnabled = false;
        private static MethodInfo zoomOutHelperPrototypeCheckMethod;

        // size of the screen, taking zooming out into account (Extended Camera Dynamics mod)

        public static int CameraWidth {
            get {
                if (!extendedCameraDynamicsEnabled && !zoomOutHelperPrototypeEnabled) return 320;
                return (Engine.Scene as Level)?.Camera.Viewport.Width ?? 320;
            }
        }

        public static int CameraHeight {
            get {
                if (!extendedCameraDynamicsEnabled && !zoomOutHelperPrototypeEnabled) return 180;
                return (Engine.Scene as Level)?.Camera.Viewport.Height ?? 180;
            }
        }

        public static int BufferWidth {
            get {
                if (!extendedCameraDynamicsEnabled && !zoomOutHelperPrototypeEnabled) return 320;
                return GameplayBuffers.Gameplay?.Width ?? 320;
            }
        }

        public static int BufferHeight {
            get {
                if (!extendedCameraDynamicsEnabled && !zoomOutHelperPrototypeEnabled) return 180;
                return GameplayBuffers.Gameplay?.Height ?? 180;
            }
        }

        private static Hook modRegister = null;

        public MaxHelpingHandModule() {
            Instance = this;
        }

        public override void Load() {
            Logger.SetLogLevel("MaxHelpingHand", LogLevel.Info);

            using (new DetourConfigContext(new DetourConfig("MaddieHelpingHand_Default")).Use()) {
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
                SpinnerBreakingBallVanilla.Load();
                ReskinnableCrystalHeart.Load();
                SetFlagOnButtonPressController.Load();
                FlagPickup.Load();
                RespawningJellyfishGeneric<RespawningJellyfish, Glider>.Load();
                SetFlagOnSpawnTrigger.Load();
                CustomSeekerBarrier.Load();
                MoreCustomNPC.Load();
                ReskinnableTouchSwitch.Load();
                ReskinnableSwitchGate.Load();
                ParallaxFadeSpeedController.Load();
                ParallaxFadeOutController.Load();
                RainbowSpinnerColorAreaController.Load();
                StylegroundFadeController.Load();
                DisableControlsController.Load();
                FancyTextTutorial.Load();
                CustomChapterNumber.Load();
                FrozenJelly.Load();
                ReverseJelly.Load();
                ReversibleRetentionBooster.Load();
                SpikeRefillController.Load();
                SideSpecificEndscreens.Load();
                Pico8FlagController.Load();

                Everest.Events.Level.OnLoadBackdrop += onLoadBackdrop;

                modRegister = new Hook(
                    typeof(Everest).GetMethod("Register"),
                    typeof(MaxHelpingHandModule).GetMethod("onModRegister", BindingFlags.NonPublic | BindingFlags.Instance), this);
            }

            typeof(LuaCutscenesUtils).ModInterop();
            typeof(EntityNameRegistry).ModInterop();
            typeof(GravityHelperImports.Interop).ModInterop();
            typeof(LuckyHelperImports.Interop).ModInterop();
        }

        public override void Initialize() {
            // either do this when mod initialize, or add speedrun tool as optional dependency
            SpeedrunToolInterop.Initialize();
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
            SpinnerBreakingBallVanilla.Unload();
            ReskinnableCrystalHeart.Unload();
            SetFlagOnButtonPressController.Unload();
            FlagPickup.Unload();
            AvBdaySpeechBubbleFixup.Unload();
            RespawningJellyfishGeneric<RespawningJellyfish, Glider>.Unload();
            ReskinnableStarRotateSpinner.Unload();
            ReskinnableStarTrackSpinner.Unload();
            SetFlagOnSpawnTrigger.Unload();
            SaveFileStrawberryGate.Unload();
            CustomSeekerBarrier.Unload();
            MoreCustomNPC.Unload();
            ReskinnableTouchSwitch.Unload();
            ReskinnableSwitchGate.Unload();
            ParallaxFadeSpeedController.Unload();
            ParallaxFadeOutController.Unload();
            RainbowSpinnerColorAreaController.Unload();
            StylegroundFadeController.Unload();
            DisableControlsController.Unload();
            FancyTextTutorial.Unload();
            CustomChapterNumber.Unload();
            FrozenJelly.Unload();
            ReverseJelly.Unload();
            ReversibleRetentionBooster.Unload();
            UpsideDownMovingPlatformGravityHelper.Unload();
            SpikeRefillController.Unload();
            MiniHeartDoorUnfixController.Unload();
            SideSpecificEndscreens.Unload();
            Pico8FlagController.Unload();
            SpeedrunToolInterop.Unload();

            Everest.Events.Level.OnLoadBackdrop -= onLoadBackdrop;

            modRegister?.Dispose();
            modRegister = null;

            if (hookedSineParallax) {
                unhookSineParallax();
            }

            if (bounceHelperLoaded) {
                unloadBounceHelper();
            }

            if (frostBreakingBallLoaded) {
                unloadFrostBreakingBall();
            }

            if (extendedCameraDynamicsHookEnabled) {
                On.Celeste.LevelLoader.StartLevel -= checkExtendedCameraDynamics;
                extendedCameraDynamicsHookEnabled = false;
            }

            if (zoomOutHelperPrototypeHookEnabled) {
                On.Celeste.LevelLoader.StartLevel -= checkZoomOutHelperPrototype;
                zoomOutHelperPrototypeCheckMethod = null;
                zoomOutHelperPrototypeHookEnabled = false;
            }
        }

        public override void LoadContent(bool firstLoad) {
            base.LoadContent(firstLoad);

            using (new DetourConfigContext(new DetourConfig("MaddieHelpingHand_Default")).Use()) {
                MultiRoomStrawberryCounter.Initialize();
                BeeFireball.LoadContent();
                SaveFileStrawberryGate.Initialize();
                ReversibleRetentionBooster.LoadContent();
            }
            HookMods();
        }

        private void onModRegister(Action<EverestModule> orig, EverestModule module) {
            orig(module);

            if ((bool) contentLoaded.GetValue(null)) {
                // the game was already initialized and a new mod was loaded at runtime:
                // make sure whe applied all mod hooks we want to apply.
                HookMods();
            }
        }

        private static void checkExtendedCameraDynamics(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self) {
            checkForExtendedCameraDynamics(self);
            orig(self);
        }

        private static void checkForExtendedCameraDynamics(LevelLoader self) {
            extendedCameraDynamicsEnabled = ExCameraAreaMetadata.TryGetCameraMetadata(self.Level.Session)?.EnableExtendedCamera ?? false;
        }

        private static void checkZoomOutHelperPrototype(On.Celeste.LevelLoader.orig_StartLevel orig, LevelLoader self) {
            zoomOutHelperPrototypeEnabled = (bool) zoomOutHelperPrototypeCheckMethod.Invoke(null, new object[] { self.Level.Session, null, null });
            orig(self);
        }

        private void HookMods() {
            using (new DetourConfigContext(new DetourConfig("MaddieHelpingHand_Default")).Use()) {
                KevinBarrier.HookMods();
                MovingFlagTouchSwitch.HookMods();
                MadelinePonytailTrigger.LoadContent();
                InstantLavaBlockerTrigger.HookMods();
                MadelineSprite.HookMods();
                ReskinnableCrystalHeart.HookMods();
                AvBdaySpeechBubbleFixup.LoadMods();
                ReskinnableStarRotateSpinner.LoadMods();
                ReskinnableStarTrackSpinner.LoadMods();
                SaveFileStrawberryGate.HookMods();
                RainbowSpinnerColorController.LoadMods();
                RainbowSpinnerColorAreaController.LoadMods();
                VivHelperGrowBlockFixup.LoadMods();
                UpsideDownMovingPlatformGravityHelper.LoadMods();
                MiniHeartDoorUnfixController.Initialize();
                CustomSeekerBarrier.LoadMods();

                if (!hookedSineParallax && Everest.Loader.DependencyLoaded(new EverestModuleMetadata() { Name = "FlaglinesAndSuch", Version = new Version(1, 4, 17) })) {
                    hookSineParallax();
                }

                if (!bounceHelperLoaded && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "BounceHelper", Version = new Version(1, 8, 0) })) {
                    loadBounceHelper();
                }

                if (!frostBreakingBallLoaded && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "FrostHelper", Version = new Version(1, 46, 0) })) {
                    loadFrostBreakingBall();
                }

                if (!extendedCameraDynamicsHookEnabled && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "ExtendedCameraDynamics", Version = new Version(1, 0, 3) })) {
                    On.Celeste.LevelLoader.StartLevel += checkExtendedCameraDynamics;
                    extendedCameraDynamicsHookEnabled = true;
                }

                if (!zoomOutHelperPrototypeHookEnabled && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "ZoomOutHelperPrototype", Version = new Version(0, 1, 1) })) {
                    On.Celeste.LevelLoader.StartLevel += checkZoomOutHelperPrototype;
                    zoomOutHelperPrototypeCheckMethod = Everest.Modules.First(mod => mod.GetType().ToString() == "Celeste.Mod.FunctionalZoomOut.FunctionalZoomOutModule")
                        .GetType().GetMethod("SessionHasZoomOut", BindingFlags.NonPublic | BindingFlags.Static);
                    zoomOutHelperPrototypeHookEnabled = true;
                }
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

        private void loadBounceHelper() {
            RespawningBounceJellyfish.LoadBounceHelper();
            bounceHelperLoaded = true;
        }

        private void unloadBounceHelper() {
            RespawningBounceJellyfish.UnloadBounceHelper();
            bounceHelperLoaded = false;
        }

        private void loadFrostBreakingBall() {
            SpinnerBreakingBallFrost.Load();
            frostBreakingBallLoaded = true;
        }

        private void unloadFrostBreakingBall() {
            SpinnerBreakingBallFrost.Unload();
            frostBreakingBallLoaded = false;
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

                // parse scroll values if both scrollX and scrollY are filled out
                Vector2? scroll = null;
                if (!string.IsNullOrEmpty(child.Attr("scrollX")) && !string.IsNullOrEmpty(child.Attr("scrollY"))) {
                    scroll = new Vector2(float.Parse(child.Attr("scrollX")), float.Parse(child.Attr("scrollY")));
                }

                return new CustomStars(starCount, string.IsNullOrEmpty(tint) ? (Color?) null : Calc.HexToColor(tint), child.Attr("spriteDirectory", "bgs/02/stars"),
                    child.AttrFloat("wrapHeight", 180f), child.AttrFloat("width", 320f), alpha, child.AttrFloat("bgAlpha", 1f), scroll);
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
                float alpha = child.AttrFloat("alpha", 1f);
                string[] colorsAsStrings = child.Attr("colors").Split(',');
                Color[] colors = new Color[colorsAsStrings.Length];
                for (int i = 0; i < colors.Length; i++) {
                    colors[i] = Calc.HexToColor(colorsAsStrings[i]) * alpha;
                }

                return new SnowCustomColors(colors, child.AttrFloat("speedMin", 40f), child.AttrFloat("speedMax", 100f), child.AttrInt("particleCount", 60));
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
