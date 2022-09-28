using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/SetFlagOnSpawnTrigger")]
    public class SetFlagOnSpawnTrigger : Trigger {
        // since the flag setting happens before the lists are updated, we cannot use the tracker...
        // so, we are doing our own!
        private static List<SetFlagOnSpawnTrigger> triggers = new List<SetFlagOnSpawnTrigger>();

        public static void Load() {
            On.Celeste.Level.LoadNewPlayer += onLoadNewPlayer;
        }

        public static void Unload() {
            On.Celeste.Level.LoadNewPlayer -= onLoadNewPlayer;
        }

        private static Player onLoadNewPlayer(On.Celeste.Level.orig_LoadNewPlayer orig, Vector2 position, PlayerSpriteMode spriteMode) {
            if (triggers.Count != 0) {
                // this method is static, so we need to access the level in another way. f
                Level level = Engine.Scene as Level;
                if (level == null) {
                    level = (Engine.Scene as LevelLoader)?.Level;
                }

                foreach (SetFlagOnSpawnTrigger trigger in triggers) {
                    trigger.setFlagOnSpawn(level, position);
                }
            }

            return orig(position, spriteMode);
        }

        private readonly string[] flags;
        private readonly string ifFlag;
        private readonly bool enable;

        public SetFlagOnSpawnTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            flags = data.Attr("flags").Split(',');
            ifFlag = data.Attr("ifFlag");
            enable = data.Bool("enable");

            triggers.Add(this);

            // make sure spawn points exactly at the bottom of the trigger also hit
            Collider.Height++;
        }

        private void setFlagOnSpawn(Level level, Vector2 spawnPoint) {
            if (Collider.Collide(spawnPoint)) {
                if (string.IsNullOrEmpty(ifFlag) || level.Session.GetFlag(ifFlag)) {
                    foreach (string flag in flags) {
                        level?.Session.SetFlag(flag, enable);
                    }
                }
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // this trigger only has work to do on spawn, it can disappear after that.
            RemoveSelf();
            triggers.Remove(this);
        }
    }
}
