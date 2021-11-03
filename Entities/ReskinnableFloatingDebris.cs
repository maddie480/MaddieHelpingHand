using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReskinnableFloatingDebris")]
    public static class ReskinnableFloatingDebris {
        private static string floatingDebrisSkin;

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            floatingDebrisSkin = entityData.Attr("texture", "scenery/debris");
            FloatingDebris debris = new FloatingDebris(entityData, offset);
            floatingDebrisSkin = null;
            return debris;
        }

        public static void Load() {
            IL.Celeste.FloatingDebris.ctor_Vector2 += modFloatingDebrisConstructor;
        }

        public static void Unload() {
            IL.Celeste.FloatingDebris.ctor_Vector2 -= modFloatingDebrisConstructor;
        }

        private static void modFloatingDebrisConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("scenery/debris"))) {
                Logger.Log("MaxHelpingHand/ReskinnableFloatingDebris", $"Reskinning floating debris @ {cursor.Index} in IL for FloatingDebris constructor");
                cursor.EmitDelegate<Func<string, string>>(orig => floatingDebrisSkin ?? orig); // use floatingDebrisSkin instead of scenery/debris, unless it is null.
            }
        }
    }
}
