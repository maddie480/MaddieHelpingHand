using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/SetFlagOnSpawnController")]
    public class SetFlagOnSpawnController : Entity {
        private static bool isRespawning = false;

        public static void Load() {
            On.Celeste.Level.LoadLevel += onLoadLevel;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= onLoadLevel;
        }

        private static void onLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            isRespawning = (playerIntro != Player.IntroTypes.Transition);
            orig(self, playerIntro, isFromLoader);
            isRespawning = false;
        }

        public SetFlagOnSpawnController(EntityData data, Vector2 offset) : base() {
            // we aren't supposed to access the level this early on, but that's a good way to be sure to set the flag
            // before Added or Awake is called on any entity in the level.
            Level level = Engine.Scene as Level;
            if (level == null) {
                level = (Engine.Scene as LevelLoader)?.Level;
            }

            if (!data.Bool("onlyOnRespawn", defaultValue: false) || isRespawning) {
                foreach (string flag in data.Attr("flag").Split(',')) {
                    level?.Session.SetFlag(flag, data.Bool("enable"));
                }
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // this entity only has work to do on spawn, it can disappear after that.
            RemoveSelf();
        }
    }
}
