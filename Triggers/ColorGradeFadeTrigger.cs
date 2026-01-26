using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System.Linq;
using Celeste.Mod.MaxHelpingHand.Module;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/ColorGradeFadeTrigger")]
    [Tracked]
    public class ColorGradeFadeTrigger : Trigger {
        public static void Load() {
            On.Celeste.Level.Update += onLevelUpdate;
        }

        public static void Unload() {
            On.Celeste.Level.Update -= onLevelUpdate;
        }

        private static void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self) {
            orig(self);

            List<Player> dummyPlayers = LuckyHelperImports.GetDummyPlayers();

            Player player = null;
            if (dummyPlayers.Count > 0)
            {
                // If one use dummyPlayer and it collides with ColorGradeFadeTrigger mostly means he wants to control the ColorGradeFadeTrigger with dummyPlayer 
                foreach (Player dummyPlayer in dummyPlayers)
                {
                    if (dummyPlayer.CollideCheck<ColorGradeFadeTrigger>())
                    {
                        player = dummyPlayer;
                        break;
                    }
                }
            }

            // check if the player is in a color grade fade trigger
            player ??= self.Tracker.GetEntity<Player>();
            ColorGradeFadeTrigger trigger = self.Tracker.GetEntities<ColorGradeFadeTrigger>().OfType<ColorGradeFadeTrigger>()
                .FirstOrDefault(t => t.evenDuringReflectionFall ? player?.Collider.Collide(t) ?? false : t.playerInside);
            if (player != null && trigger != null) {
                // the game fades from lastColorGrade to Session.ColorGrade using colorGradeEase as a lerp value.
                // let's hijack that!
                float positionLerp = trigger.GetPositionLerp(player, trigger.direction);
                if (positionLerp > 0.5f) {
                    // we are closer to B. let B be the target color grade when player exits the trigger / dies in it
                    self.lastColorGrade = trigger.colorGradeA;
                    self.Session.ColorGrade = trigger.colorGradeB;
                    self.colorGradeEase = positionLerp;
                } else {
                    // we are closer to A. let A be the target color grade when player exits the trigger / dies in it
                    self.lastColorGrade = trigger.colorGradeB;
                    self.Session.ColorGrade = trigger.colorGradeA;
                    self.colorGradeEase = 1 - positionLerp;
                }
                self.colorGradeEaseSpeed = 1f;
            }
        }


        private string colorGradeA;
        private string colorGradeB;
        private PositionModes direction;
        private bool evenDuringReflectionFall;

        private bool playerInside = false;

        public ColorGradeFadeTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            colorGradeA = data.Attr("colorGradeA");
            colorGradeB = data.Attr("colorGradeB");
            direction = data.Enum<PositionModes>("direction");
            evenDuringReflectionFall = data.Bool("evenDuringReflectionFall", true); // true by default for backwards compatibility
        }

        public override void OnEnter(Player player) {
            playerInside = true;
        }

        public override void OnLeave(Player player) {
            playerInside = false;
        }
    }
}
