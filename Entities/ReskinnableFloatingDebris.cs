using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReskinnableFloatingDebris")]
    public static class ReskinnableFloatingDebris {
        private static string floatingDebrisSkin;
        private static int? floatingDebrisWidth = null;
        private static int? floatingDebrisHeight = null;

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            floatingDebrisSkin = entityData.Attr("texture", "scenery/debris");
            floatingDebrisWidth = entityData.Int("debrisWidth", 8);
            floatingDebrisHeight = entityData.Int("debrisHeight", 8);

            FloatingDebris debris = new FloatingDebris(entityData, offset);

            // Depth option
            debris.Depth = entityData.Int("depth", -5);

            // alter hitbox (useful to fit modded debris sizes)
            if (floatingDebrisWidth != null && floatingDebrisHeight != null) {
                debris.Collider = new Hitbox(
                    floatingDebrisWidth.Value + 4, floatingDebrisHeight.Value + 4,
                    -(floatingDebrisWidth.Value / 2) - 2, -(floatingDebrisHeight.Value / 2) - 2);
            }

            // Interact With Player option
            debris.Collidable = entityData.Bool("interactWithPlayer", defaultValue: true);

            // Rotate Speed option
            if (!string.IsNullOrEmpty(entityData.Attr("rotateSpeed"))) {
                debris.rotateSpeed = float.Parse(entityData.Attr("rotateSpeed"));
            }

            // Scroll X and Scroll Y options
            float scrollX = entityData.Float("scrollX");
            float scrollY = entityData.Float("scrollY");
            if (scrollX != 0 || scrollY != 0) {
                DebrisParallaxThingy positionUpdater = new DebrisParallaxThingy(debris.Position, new Vector2(scrollX, scrollY));
                debris.Add(positionUpdater);
                debris.Add(new TransitionListener() { OnIn = _ => positionUpdater.Update(), OnOut = _ => positionUpdater.Update() });
            }

            // Floating option
            if (!entityData.Bool("floating", defaultValue: true)) {
                SineWave sine = debris.Get<SineWave>();
                debris.Remove(sine);
                sine.Reset();
            }

            floatingDebrisSkin = null;
            floatingDebrisWidth = null;
            floatingDebrisHeight = null;

            return debris;
        }

        public static void Load() {
            IL.Celeste.FloatingDebris.ctor_Vector2 += ilHookDebrisConstructor;
        }

        public static void Unload() {
            IL.Celeste.FloatingDebris.ctor_Vector2 -= ilHookDebrisConstructor;
        }

        private static void ilHookDebrisConstructor(ILContext il) {
            // change texture
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("scenery/debris"))) {
                Logger.Log("MaxHelpingHand/ReskinnableFloatingDebris", $"Reskinning floating debris @ {cursor.Index} in IL for FloatingDebris constructor");
                cursor.EmitDelegate<Func<string, string>>(reskinDebris); // use floatingDebrisSkin instead of scenery/debris, unless it is null.
            }

            // change width and height
            cursor.Index = 0;
            if (cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchMul(),
                instr => instr.MatchLdcI4(0),
                instr => instr.MatchLdcI4(8),
                instr => instr.MatchLdcI4(8))) {
                Logger.Log("MaxHelpingHand/ReskinnableFloatingDebris", $"Resizing floating debris @ {cursor.Index} in IL for FloatingDebris constructor");

                // the last 8 here refers to the height (new MTexture(parent, x, y, width, <<height>>)).
                cursor.EmitDelegate<Func<int, int>>(modDebrisHeight);

                // all 8's before that are referring to the width.
                cursor.Index--;
                while (cursor.TryGotoPrev(MoveType.After, instr => instr.MatchLdcI4(8))) {
                    int index = cursor.Index;
                    cursor.EmitDelegate<Func<int, int>>(modDebrisWidth);
                    cursor.Index = index - 1;
                }
            }
        }
        private static string reskinDebris(string orig) {
            return floatingDebrisSkin ?? orig;
        }

        private static int modDebrisHeight(int orig) {
            return floatingDebrisHeight ?? orig;
        }

        private static int modDebrisWidth(int orig) {
            return floatingDebrisWidth ?? orig;
        }

        private class DebrisParallaxThingy : Component {
            private readonly Vector2 position, scroll;
            private Vector2? levelPosition;

            public DebrisParallaxThingy(Vector2 position, Vector2 scroll) : base(active: true, visible: false) {
                this.position = position;
                this.scroll = scroll;
            }

            public override void Update() {
                base.Update();

                levelPosition ??= (Entity.Scene as Level).Session.LevelData.Position;

                Camera levelCamera = (Entity?.Scene as Level)?.Camera;
                if (levelCamera != null) {
                    Entity.Position = position + (levelCamera.Position - levelPosition.Value) * scroll;
                }
            }
        }
    }
}
