using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// An indicator for the strawberry seeds Madeline has and still has to get, that appears above Madeline's head.
    /// </summary>
    [Tracked]
    class MultiRoomStrawberryCounter : Entity {
        private const int normalDepth = (Depths.FGTerrain + Depths.FGDecals) / 2; // between fg tiles and fg decals
        private const int depthInFrontOfSolids = Depths.FakeWalls - 1; // in front of fake walls

        // strawberry count is hidden during intros and cutscenes, when player doesn't have control.
        private static readonly HashSet<int> hiddenPlayerStates = new HashSet<int> { Player.StIntroJump, Player.StIntroMoonJump, Player.StIntroRespawn,
            Player.StIntroThinkForABit, Player.StIntroWakeUp, Player.StIntroWalk, Player.StReflectionFall, Player.StDummy };

        private static MTexture[] numbers;

        public static void Initialize() {
            // extract numbers from the PICO-8 font that ships with the game.
            MTexture source = GFX.Game["pico8/font"];
            numbers = new MTexture[11];
            int index = 0;
            for (int i = 104; index < 4; i += 4) {
                numbers[index++] = source.GetSubtexture(i, 0, 3, 5);
            }
            for (int i = 0; index < 10; i += 4) {
                numbers[index++] = source.GetSubtexture(i, 6, 3, 5);
            }

            // 10 will be the "/" character.
            numbers[10] = source.GetSubtexture(24, 6, 3, 5);
        }

        private float timer = 5;
        private readonly string text;

        public MultiRoomStrawberryCounter(string text) {
            AddTag(Tags.Persistent); // this entity isn't bound to the screen it was spawned in, keep it when transitioning to another level.
            this.text = text;
        }

        public override void Update() {
            base.Update();

            timer -= Engine.DeltaTime;
            if (timer <= 0f) {
                // timer was displayed for 5 seconds: remove it.
                RemoveSelf();
            }

            // if the indicator overlaps with a solid that's in front of everything (exit blocks etc), send it in front of them.
            // otherwise, have it between fg decals and fg tiles
            if (Collider != null && CollideAll<Solid>().Any(solid => solid.Depth < normalDepth)) {
                Depth = depthInFrontOfSolids;
            } else {
                Depth = normalDepth;
            }
        }

        public override void Render() {
            base.Render();

            float minX = float.MaxValue, maxX = float.MaxValue, minY = float.MaxValue, maxY = float.MaxValue;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                if (hiddenPlayerStates.Contains(player.StateMachine.State)) {
                    // the player is in one of the states that should have the berry counter hidden.
                    return;
                }

                // draw the counter.
                int totalWidth = text.Length * 4 - 1;
                for (int i = 0; i < text.Length; i++) {
                    Vector2 position = player.Center + new Vector2(-totalWidth / 2 + i * 4, -18f);
                    int index = text.ToCharArray()[i] == '/' ? 10 : text.ToCharArray()[i] - '0';
                    numbers[index].DrawOutline(position, new Vector2(0f, 0.5f));

                    if (minX == float.MaxValue) {
                        minX = maxX = position.X;
                        minY = maxY = position.Y;
                    } else {
                        minX = Math.Min(minX, position.X);
                        maxX = Math.Max(maxX, position.X);
                        minY = Math.Min(minY, position.Y);
                        maxY = Math.Max(maxY, position.Y);
                    }
                }
            }

            if (minX != float.MaxValue) {
                Collider = new Hitbox(maxX - minX + 5, maxY - minY + 6, minX - 1, minY - 1);
            } else {
                Collider = null;
            }
        }
    }
}
