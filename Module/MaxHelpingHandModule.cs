using Celeste.Mod.MaxHelpingHand.Effects;
using Celeste.Mod.MaxHelpingHand.Entities;
using Celeste.Mod.MaxHelpingHand.Triggers;
using System;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandModule : EverestModule {
        public static MaxHelpingHandModule Instance { get; private set; }

        public override Type SessionType => typeof(MaxHelpingHandSession);
        public MaxHelpingHandSession Session => (MaxHelpingHandSession) _Session;

        public MaxHelpingHandModule() {
            Instance = this;
        }

        public override void Load() {
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

            Everest.Events.Level.OnLoadBackdrop -= onLoadBackdrop;
        }

        private Backdrop onLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element above) {
            if (child.Name.Equals("MaxHelpingHand/HeatWaveNoColorGrade", StringComparison.OrdinalIgnoreCase)) {
                return new HeatWaveNoColorGrade(child.AttrBool("controlColorGradeWhenActive"));
            }
            if (child.Name.Equals("MaxHelpingHand/BlackholeCustomColors", StringComparison.OrdinalIgnoreCase)) {
                return BlackholeCustomColors.CreateBlackholeWithCustomColors(child);
            }
            return null;
        }

        public override void PrepareMapDataProcessors(MapDataFixup context) {
            base.PrepareMapDataProcessors(context);

            context.Add<MaxHelpingHandMapDataProcessor>();
        }
    }
}
