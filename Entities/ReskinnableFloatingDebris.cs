using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReskinnableFloatingDebris")]
    public static class ReskinnableFloatingDebris {
        private static string floatingDebrisSkin;
        private static int? floatingDebrisDepth = null;

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            floatingDebrisSkin = entityData.Attr("texture", "scenery/debris");
            floatingDebrisDepth = entityData.Int("depth", -5);

            FloatingDebris debris = new FloatingDebris(entityData, offset);

            floatingDebrisSkin = null;
            floatingDebrisDepth = null;

            return debris;
        }

        public static void Load() {
            IL.Celeste.FloatingDebris.ctor_Vector2 += modFloatingDebrisTexture;
            On.Celeste.FloatingDebris.ctor_Vector2 += modFloatingDebrisDepth;
        }

        public static void Unload() {
            IL.Celeste.FloatingDebris.ctor_Vector2 -= modFloatingDebrisTexture;
            On.Celeste.FloatingDebris.ctor_Vector2 -= modFloatingDebrisDepth;
        }

        private static void modFloatingDebrisTexture(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("scenery/debris"))) {
                Logger.Log("MaxHelpingHand/ReskinnableFloatingDebris", $"Reskinning floating debris @ {cursor.Index} in IL for FloatingDebris constructor");
                cursor.EmitDelegate<Func<string, string>>(orig => floatingDebrisSkin ?? orig); // use floatingDebrisSkin instead of scenery/debris, unless it is null.
            }
        }

        private static void modFloatingDebrisDepth(On.Celeste.FloatingDebris.orig_ctor_Vector2 orig, FloatingDebris self, Vector2 position) {
            orig(self, position);

            if (floatingDebrisDepth != null) {
                self.Depth = floatingDebrisDepth.Value;
            }
        }
    }
}
