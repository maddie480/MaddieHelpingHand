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
        private static int? floatingDebrisWidth = null;
        private static int? floatingDebrisHeight = null;

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            floatingDebrisSkin = entityData.Attr("texture", "scenery/debris");
            floatingDebrisDepth = entityData.Int("depth", -5);
            floatingDebrisWidth = entityData.Int("debrisWidth", 8);
            floatingDebrisHeight = entityData.Int("debrisHeight", 8);

            FloatingDebris debris = new FloatingDebris(entityData, offset);

            floatingDebrisSkin = null;
            floatingDebrisDepth = null;
            floatingDebrisWidth = null;
            floatingDebrisHeight = null;

            debris.Collidable = entityData.Bool("interactWithPlayer", defaultValue: true);

            return debris;
        }

        public static void Load() {
            IL.Celeste.FloatingDebris.ctor_Vector2 += ilHookDebrisConstructor;
            On.Celeste.FloatingDebris.ctor_Vector2 += onHookDebrisConstructor;
        }

        public static void Unload() {
            IL.Celeste.FloatingDebris.ctor_Vector2 -= ilHookDebrisConstructor;
            On.Celeste.FloatingDebris.ctor_Vector2 -= onHookDebrisConstructor;
        }

        private static void ilHookDebrisConstructor(ILContext il) {
            // change texture
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("scenery/debris"))) {
                Logger.Log("MaxHelpingHand/ReskinnableFloatingDebris", $"Reskinning floating debris @ {cursor.Index} in IL for FloatingDebris constructor");
                cursor.EmitDelegate<Func<string, string>>(orig => floatingDebrisSkin ?? orig); // use floatingDebrisSkin instead of scenery/debris, unless it is null.
            }

            // change width and height
            cursor.Index = 0;
            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchMul(),
                instr => instr.MatchLdcI4(0),
                instr => instr.MatchLdcI4(8),
                instr => instr.MatchLdcI4(8))) {
                Logger.Log("MaxHelpingHand/ReskinnableFloatingDebris", $"Resizing floating debris @ {cursor.Index} in IL for FloatingDebris constructor");

                // the last 8 here refers to the height (new MTexture(parent, x, y, width, <<height>>)).
                cursor.EmitDelegate<Func<int, int>>(orig => floatingDebrisHeight ?? orig);

                // all 8's before that are referring to the width.
                cursor.Index--;
                while (cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdcI4(8))) {
                    int index = cursor.Index;
                    cursor.EmitDelegate<Func<int, int>>(orig => floatingDebrisWidth ?? orig);
                    cursor.Index = index - 1;
                }
            }
        }

        private static void onHookDebrisConstructor(On.Celeste.FloatingDebris.orig_ctor_Vector2 orig, FloatingDebris self, Vector2 position) {
            orig(self, position);

            // alter depth
            if (floatingDebrisDepth != null) {
                self.Depth = floatingDebrisDepth.Value;
            }

            // alter hitbox (useful to fit modded debris sizes)
            if (floatingDebrisWidth != null && floatingDebrisHeight != null) {
                self.Collider = new Hitbox(
                    floatingDebrisWidth.Value + 4, floatingDebrisHeight.Value + 4,
                    -(floatingDebrisWidth.Value / 2) - 2, -(floatingDebrisHeight.Value / 2) - 2);
            }
        }
    }
}
