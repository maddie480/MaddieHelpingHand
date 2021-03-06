using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/HintDecal")]
    class HintDecal : Decal {
        public static void Load() {
            Everest.Events.CustomBirdTutorial.OnParseCommand += parseBirdCommand;
        }

        public static void Unload() {
            Everest.Events.CustomBirdTutorial.OnParseCommand -= parseBirdCommand;
        }

        private static object parseBirdCommand(string command) {
            if (command == "ShowHints") {
                return MaxHelpingHandModule.Instance.Settings.ShowHints.Button;
            }
            return null;
        }

        public HintDecal(EntityData data, Vector2 offset) :
            base(data.Attr("texture"), data.Position + offset, new Vector2(data.Float("scaleX"), data.Float("scaleY")), data.Bool("foreground") ? Depths.FGDecals : Depths.BGDecals) {

            Visible = false;
        }

        public override void Update() {
            base.Update();
            Visible = MaxHelpingHandModule.Instance.Settings.ShowHints.Check;
        }
    }
}
