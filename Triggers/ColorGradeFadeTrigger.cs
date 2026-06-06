using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using System.Linq;
using MonoMod.Cil;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/ColorGradeFadeTrigger")]
    [Tracked]
    public class ColorGradeFadeTrigger : Trigger {
        public static void Load() {
            IL.Celeste.Level.Update += modLevelUpdate;
        }

        public static void Unload() {
            IL.Celeste.Level.Update -= modLevelUpdate;
        }

        private static void modLevelUpdate(ILContext il) {
            ILCursor cursor = new(il);

            ILLabel skipColorGradeUpdate = null;
            if (cursor.TryGotoNextBestFit(MoveType.Before,
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld<Level>("lastColorGrade"),
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld<Level>("Session"),
                instr => instr.MatchLdfld<Session>("ColorGrade"),
                instr => instr.MatchCall<string>("op_Inequality"),
                instr => instr.MatchBrfalse(out skipColorGradeUpdate))) {
                cursor.EmitLdarg0();
                cursor.EmitDelegate(modColorGradeUpdate);
                cursor.EmitBrtrue(skipColorGradeUpdate!);
            }
        }

        private static bool modColorGradeUpdate(Level level) {
            return level.Tracker.GetEntities<ColorGradeFadeTrigger>().Cast<ColorGradeFadeTrigger>().Any(t => t.Triggered);
        }

        private string colorGradeA;
        private string colorGradeB;
        private PositionModes direction;
        private bool evenDuringReflectionFall;

        public ColorGradeFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            colorGradeA = data.Attr("colorGradeA");
            colorGradeB = data.Attr("colorGradeB");
            direction = data.Enum<PositionModes>("direction");
            evenDuringReflectionFall = data.Bool("evenDuringReflectionFall", true); // true by default for backwards compatibility

            if (evenDuringReflectionFall) {
                Add(new TriggerDuringReflectionFall());
            }
        }

        public override void OnStay(Player player) {
            Level level = SceneAs<Level>();

            float positionLerp = GetPositionLerp(player, direction);
            if (positionLerp > 0.5f) {
                // we are closer to B. let B be the target color grade when player exits the trigger / dies in it
                level.lastColorGrade = colorGradeA;
                level.Session.ColorGrade = colorGradeB;
                level.colorGradeEase = positionLerp;
            } else {
                // we are closer to A. let A be the target color grade when player exits the trigger / dies in it
                level.lastColorGrade = colorGradeB;
                level.Session.ColorGrade = colorGradeA;
                level.colorGradeEase = 1 - positionLerp;
            }
            level.colorGradeEaseSpeed = 1f;
        }
    }
}
