using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/MultiNodeMovingPlatform")]
    class MultiNodeMovingPlatform : JumpThru {
        private enum Mode {
            BackAndForth, BackAndForthNoPause, Loop, LoopNoPause, TeleportBack
        }

        // settings
        private Vector2[] nodes;
        private float moveTime;
        private float pauseTime;
        private string overrideTexture;
        private Mode mode;
        private bool easing;

        private MTexture[] textures;
        private float[] nodePercentages;

        private string lastSfx;
        private SoundSource sfx;

        // status tracking
        private float pauseTimer;
        private int prevNodeIndex = 0;
        private int nextNodeIndex = 1;
        private float percent;
        private int direction = 1;

        // sinking effect status tracking
        private float addY;
        private float sinkTimer;

        public MultiNodeMovingPlatform(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, false) {

            // read attributes
            moveTime = data.Float("moveTime", 2f);
            pauseTime = data.Float("pauseTime");
            overrideTexture = data.Attr("texture", "default");
            mode = data.Enum("mode", Mode.Loop);
            easing = data.Bool("easing", true);

            // read nodes
            nodes = new Vector2[data.Nodes.Length + 1];
            nodes[0] = data.Position + offset;
            for (int i = 0; i < data.Nodes.Length; i++) {
                nodes[i + 1] = data.Nodes[i] + offset;
            }

            // set up sounds and lighting
            Add(sfx = new SoundSource());
            SurfaceSoundIndex = 5;
            lastSfx = (Math.Sign(nodes[0].X - nodes[1].X) > 0 || Math.Sign(nodes[0].Y - nodes[1].Y) > 0) ?
                "event:/game/03_resort/platform_horiz_left" : "event:/game/03_resort/platform_horiz_right";

            Add(new LightOcclude(0.2f));

            if (mode == Mode.BackAndForthNoPause || mode == Mode.LoopNoPause) {
                nodePercentages = new float[mode == Mode.LoopNoPause ? nodes.Length : nodes.Length - 1];
                float totalDistance = 0;

                // compute the distance between each node.
                for (int i = 0; i < nodes.Length - 1; i++) {
                    float distance = Vector2.Distance(nodes[i], nodes[i + 1]);
                    nodePercentages[i] = totalDistance + distance;
                    totalDistance += distance;
                }

                // if looping, also compute the distance between the last node and the first one.
                if (mode == Mode.LoopNoPause) {
                    float distance = Vector2.Distance(nodes[nodes.Length - 1], nodes[0]);
                    nodePercentages[nodes.Length - 1] = totalDistance + distance;
                    totalDistance += distance;
                }

                // turn them into percentages.
                for (int i = 0; i < nodePercentages.Length; i++) {
                    nodePercentages[i] /= totalDistance;
                }
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // read the matching texture
            if (string.IsNullOrEmpty(overrideTexture)) {
                overrideTexture = AreaData.Get(scene).WoodPlatform;
            }
            MTexture platformTexture = GFX.Game["objects/woodPlatform/" + overrideTexture];
            textures = new MTexture[platformTexture.Width / 8];
            for (int i = 0; i < textures.Length; i++) {
                textures[i] = platformTexture.GetSubtexture(i * 8, 0, 8, 8);
            }

            // draw lines between all nodes
            if (Visible) {
                Vector2 lineOffset = new Vector2(Width, Height + 4f) / 2f;
                scene.Add(new MovingPlatformLine(nodes[0] + lineOffset, nodes[1] + lineOffset));
                if (nodes.Length > 2) {
                    for (int i = 1; i < nodes.Length - 1; i++) {
                        scene.Add(new MovingPlatformLine(nodes[i] + lineOffset, nodes[i + 1] + lineOffset));
                    }

                    if (mode == Mode.Loop || mode == Mode.LoopNoPause) {
                        scene.Add(new MovingPlatformLine(nodes[nodes.Length - 1] + lineOffset, nodes[0] + lineOffset));
                    }
                }
            }
        }

        public override void Render() {
            textures[0].Draw(Position);
            for (int i = 8; i < Width - 8f; i += 8) {
                textures[1].Draw(Position + new Vector2(i, 0f));
            }
            textures[3].Draw(Position + new Vector2(Width - 8f, 0f));
            textures[2].Draw(Position + new Vector2(Width / 2f - 4f, 0f));
        }

        public override void OnStaticMoverTrigger(StaticMover sm) {
            sinkTimer = 0.4f;
        }

        /// <summary>
        /// Makes this platform animate another entity, instead of ... serving as a platform.
        /// </summary>
        /// <param name="staticMover">The static mover associated to the entity to animate</param>
        internal void AnimateObject(StaticMover staticMover) {
            staticMovers.Add(staticMover);
            Visible = false;
            Collidable = false;
        }

        public override void Update() {
            base.Update();

            // manage the "sinking" effect when the player is on the platform
            if (HasPlayerRider()) {
                sinkTimer = 0.2f;
                addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
            } else if (sinkTimer > 0f) {
                sinkTimer -= Engine.DeltaTime;
                addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
            } else {
                addY = Calc.Approach(addY, 0f, 20f * Engine.DeltaTime);
            }

            if (pauseTimer > 0f) {
                // the platform is currently paused at a node.
                pauseTimer -= Engine.DeltaTime;

                // still update the position to be sure to apply addY.
                MoveTo(nodes[prevNodeIndex] + new Vector2(0f, addY));
                return;
            } else {
                if (percent == 0 && Visible) {
                    // the platform started moving. play sound
                    if (lastSfx == "event:/game/03_resort/platform_horiz_left") {
                        sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_right");
                    } else {
                        sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_left");
                    }
                }

                // move forward...
                percent = Calc.Approach(percent, 1f, Engine.DeltaTime / moveTime);

                if (mode == Mode.BackAndForthNoPause || mode == Mode.LoopNoPause) {
                    // NO PAUSE MODES: the "percentage" is the progress for the whole track, including all nodes.

                    if (percent == 1f) {
                        // we reached the last node.
                        // pause, then start over.
                        percent = 0f;
                        pauseTimer = pauseTime;

                        if (mode == Mode.BackAndForthNoPause) {
                            // the current node we're stopped at is either the last node, or the first one, depending on the direction.
                            prevNodeIndex = direction == 1 ? nodes.Length - 1 : 0;

                            // go the other way round now.
                            direction = -direction;
                        }

                        MoveTo(nodes[prevNodeIndex] + new Vector2(0f, addY));
                    } else {
                        // OTHER MODES: the "percentage" is the progress between the current node and the next one.

                        float easedPercentage = applyEase(direction == 1 ? percent : 1 - percent);

                        // for example, if node percentages are 0.2 and 1, and easedPercentage is 0.6, nextNodeIndex = 1.
                        int nextNodeIndex = 0;
                        while (nodePercentages[nextNodeIndex] < easedPercentage) {
                            nextNodeIndex++;
                        }

                        // in this case, previousNodePercentage = 0.2 and nextNodePercentage = 1. ClampedMap will remap 0.6 to 0.5 since this is halfway between 0.2 and 1.
                        float previousNodePercentage = nextNodeIndex == 0 ? 0 : nodePercentages[nextNodeIndex - 1];
                        float nextNodePercentage = nodePercentages[nextNodeIndex];

                        MoveTo(Vector2.Lerp(nodes[nextNodeIndex], nodes[(nextNodeIndex + 1) % nodes.Length],
                            Calc.ClampedMap(easedPercentage, previousNodePercentage, nextNodePercentage)) + new Vector2(0f, addY));
                    }
                } else {
                    // lerp between the previous node and the next one.
                    MoveTo(Vector2.Lerp(nodes[prevNodeIndex], nodes[nextNodeIndex], applyEase(percent)) + new Vector2(0f, addY));

                    if (percent == 1f) {
                        // reached the end. start waiting before moving again, and switch the target to the next node.
                        prevNodeIndex = nextNodeIndex;
                        nextNodeIndex = prevNodeIndex + direction;
                        if (nextNodeIndex < 0) {
                            // done moving back, let's move forth again
                            nextNodeIndex = 1;
                            direction = 1;
                        } else if (nextNodeIndex >= nodes.Length) {
                            // reached the last node
                            if (mode == Mode.Loop) {
                                // go to the first node
                                nextNodeIndex = 0;
                            } else if (mode == Mode.TeleportBack) {
                                // go back to the first node instantly.
                                prevNodeIndex = 0;
                                nextNodeIndex = 1;
                            } else if (mode == Mode.BackAndForth) {
                                // start going back
                                nextNodeIndex -= 2;
                                direction = -1;
                            }
                        }
                        percent = 0;
                        pauseTimer = pauseTime;
                    }
                }
            }
        }

        /// <summary>
        /// Apply easing if the "easing" attribute is true, else don't modify the value.
        /// </summary>
        private float applyEase(float rawValue) {
            if (easing) {
                return Ease.SineInOut(rawValue);
            }

            return rawValue;
        }
    }
}
