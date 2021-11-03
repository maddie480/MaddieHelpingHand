using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/MultiNodeFlagSwitchGate")]
    public class MultiNodeFlagSwitchGate : Solid {
        private static Dictionary<string, Ease.Easer> easeTypes = new Dictionary<string, Ease.Easer> {
            { "Linear", Ease.Linear },
            { "SineIn", Ease.SineIn },
            { "SineOut", Ease.SineOut },
            { "SineInOut", Ease.SineInOut },
            { "QuadIn", Ease.QuadIn },
            { "QuadOut", Ease.QuadOut },
            { "QuadInOut", Ease.QuadInOut },
            { "CubeIn", Ease.CubeIn },
            { "CubeOut", Ease.CubeOut },
            { "CubeInOut", Ease.CubeInOut },
            { "QuintIn", Ease.QuintIn },
            { "QuintOut", Ease.QuintOut },
            { "QuintInOut", Ease.QuintInOut },
            { "BackIn", Ease.BackIn },
            { "BackOut", Ease.BackOut },
            { "BackInOut", Ease.BackInOut },
            { "ExpoIn", Ease.ExpoIn },
            { "ExpoOut", Ease.ExpoOut },
            { "ExpoInOut", Ease.ExpoInOut },
            { "BigBackIn", Ease.BigBackIn },
            { "BigBackOut", Ease.BigBackOut },
            { "BigBackInOut", Ease.BigBackInOut },
            { "ElasticIn", Ease.ElasticIn },
            { "ElasticOut", Ease.ElasticOut },
            { "ElasticInOut", Ease.ElasticInOut },
            { "BounceIn", Ease.BounceIn },
            { "BounceOut", Ease.BounceOut },
            { "BounceInOut", Ease.BounceInOut }
        };

        private readonly Vector2 startPos;
        private readonly Vector2[] nodes;
        private readonly string[] flags;
        private readonly bool resetFlags;
        private readonly bool canReturn;
        private readonly bool progressionMode;
        private readonly float shakeTime;
        private readonly float moveTime;
        private readonly Ease.Easer easer;
        private readonly Color inactiveColor;
        private readonly Color activeColor;
        private readonly Color finishColor;
        private readonly bool persistent;

        private Sprite icon;
        private MTexture texture;
        private Vector2 iconOffset;
        private Wiggler wiggler;
        private SoundSource openSfx;

        private bool moving;
        private bool cancelMoving;
        private int nodeIndex;
        private bool[] wasEnabled;

        public MultiNodeFlagSwitchGate(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false) {
            // parse all options
            nodes = data.NodesOffset(offset);
            flags = data.Attr("flags").Split(',');
            if (flags.Length == 1 && flags[0] == "")
                flags = new string[0];
            resetFlags = data.Bool("resetFlags", true);
            canReturn = data.Bool("canReturn", true);
            shakeTime = data.Float("shakeTime", 0.5f);
            moveTime = data.Float("moveTime", 2f);
            progressionMode = data.Bool("progressionMode");
            easer = easeTypes[data.Attr("easing", "CubeOut")];
            inactiveColor = data.HexColor("inactiveColor", Calc.HexToColor("5fcde4"));
            activeColor = data.HexColor("activeColor", Color.White);
            finishColor = data.HexColor("finishColor", Calc.HexToColor("f141df"));
            persistent = data.Bool("persistent", true);
            var spriteName = data.Attr("sprite", "block");
            var iconName = data.Attr("icon", "vanilla");

            startPos = Position;
            wasEnabled = new bool[flags.Length];

            // initialize the icon
            icon = new Sprite(GFX.Game, iconName == "vanilla" ? "objects/switchgate/icon" : $"objects/MaxHelpingHand/flagSwitchGate/{iconName}/icon");
            icon.Add("spin", "", 0.1f, "spin");
            icon.Play("spin");
            icon.Rate = 0f;
            icon.Color = inactiveColor;
            icon.Position = (iconOffset = new Vector2(Width / 2f, Height / 2f));
            icon.CenterOrigin();
            Add(icon);

            // initialize the gate texture
            string blockSpriteName = data.Attr("sprite", "block");
            texture = GFX.Game["objects/switchgate/" + blockSpriteName];

            // initialize other components
            Add(wiggler = Wiggler.Create(0.5f, 4f, delegate (float f) {
                icon.Scale = Vector2.One * (1f + f);
            }));
            Add(openSfx = new SoundSource());
            Add(new LightOcclude(0.5f));
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (!persistent) {
                // reset all session flags.
                foreach (string flag in flags) {
                    (scene as Level).Session.SetFlag(flag, false);
                }
            }

            // check if we should move to a further node right away.
            nodeIndex = getNode();
            if (nodeIndex > 0) {
                MoveTo(nodes[nodeIndex - 1]);
                icon.Rate = 0f;
                icon.SetAnimationFrame(0);
                icon.Color = finishColor;
                resetAllFlags();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool InView() {
            Camera camera = (Scene as Level).Camera;
            return Position.X + Width > camera.X - 16f && Position.Y + Height > camera.Y - 16f && Position.X < camera.X + 320f && Position.Y < camera.Y + 180f;
        }

        public override void Render() {
            if (!InView()) return;

            int widthInTiles = (int) Collider.Width / 8 - 1;
            int heightInTiles = (int) Collider.Height / 8 - 1;

            Vector2 renderPos = new Vector2(Position.X + Shake.X, Position.Y + Shake.Y);
            Texture2D baseTexture = texture.Texture.Texture;
            int clipBaseX = texture.ClipRect.X;
            int clipBaseY = texture.ClipRect.Y;

            Rectangle clipRect = new Rectangle(clipBaseX, clipBaseY, 8, 8);

            for (int i = 0; i <= widthInTiles; i++) {
                clipRect.X = clipBaseX + ((i < widthInTiles) ? i == 0 ? 0 : 8 : 16);
                for (int j = 0; j <= heightInTiles; j++) {
                    int tilePartY = (j < heightInTiles) ? j == 0 ? 0 : 8 : 16;
                    clipRect.Y = tilePartY + clipBaseY;
                    Draw.SpriteBatch.Draw(baseTexture, renderPos, clipRect, Color.White);
                    renderPos.Y += 8f;
                }
                renderPos.X += 8f;
                renderPos.Y = Position.Y + Shake.Y;
            }

            icon.Position = iconOffset + Shake;
            icon.DrawOutline();

            base.Render();
        }

        public override void Update() {
            var newIndex = getNode();
            if (newIndex != nodeIndex) {
                // move to new node
                nodeIndex = newIndex;
                Add(new Coroutine(Sequence(nodeIndex > 0 ? nodes[nodeIndex - 1] : startPos)));
                resetAllFlags();
            }
            base.Update();
        }

        private int getNode() {
            int firstEnabled = 0;
            int lastEnabled = 0;
            int firstNewEnabled = 0;

            for (int i = 0; i < Math.Min(nodes.Length, flags.Length); i++) {
                if (SceneAs<Level>().Session.GetFlag(flags[i])) {
                    if (firstEnabled == 0) {
                        // we found the first enabled flag!
                        firstEnabled = i + 1;
                    }

                    if (!wasEnabled[i]) {
                        wasEnabled[i] = true;
                        if (firstNewEnabled == 0) {
                            // we found the first enabled flag that wasn't enabled before!
                            firstNewEnabled = i + 1;
                        }
                    }

                    lastEnabled = i + 1;
                } else {
                    wasEnabled[i] = false;
                    if (progressionMode) {
                        // stop checking further flags, we want the first flags to stay set.
                        break;
                    }
                }
            }

            if (progressionMode && lastEnabled > 0) {
                // jump to the last enabled flag in the progression.
                return lastEnabled;

            } else if (firstNewEnabled > 0) {
                // jump to newly enabled flags.
                return firstNewEnabled;

            } else if (!canReturn || (nodeIndex > 0 && wasEnabled[nodeIndex - 1])) {
                // do not move.
                return nodeIndex;

            } else {
                // jump back to the first enabled flag.
                return firstEnabled;
            }
        }

        private void resetAllFlags() {
            if (resetFlags && !progressionMode) {
                // reset all flags except the current one.
                for (int i = 0; i < Math.Min(nodes.Length, flags.Length); i++) {
                    if (nodeIndex != i + 1) {
                        SceneAs<Level>().Session.SetFlag(flags[i], false);
                    }
                }
            }
        }

        private IEnumerator Sequence(Vector2 node) {
            while (moving) {
                // cancel the current move, and wait for the move to be effectively cancelled
                cancelMoving = true;
                yield return null;
            }
            cancelMoving = false;
            moving = true;

            Vector2 start = Position;
            wiggler.Stop();
            icon.Scale = Vector2.One;

            if (node != start) {
                yield return 0.1f;

                openSfx.Play("event:/game/general/touchswitch_gate_open");

                // shake
                if (shakeTime > 0f) {
                    StartShaking(shakeTime);
                    while (icon.Rate < 1f) {
                        var lastColor = icon.Color;
                        icon.Color = Color.Lerp(lastColor, activeColor, icon.Rate);
                        icon.Rate += Engine.DeltaTime / shakeTime;
                        yield return null;
                    }
                } else {
                    icon.Rate = 1f;
                    icon.Color = activeColor;
                }

                yield return 0.1f;

                if (cancelMoving) {
                    moving = false;
                    yield break;
                }

                // move and emit particles
                int particleAt = 0;
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, easer, moveTime, start: true);
                bool waiting = true;
                tween.OnUpdate = delegate (Tween t) {
                    MoveTo(Vector2.Lerp(start, node, t.Eased));
                    if (Scene.OnInterval(0.1f)) {
                        particleAt++;
                        particleAt %= 2;
                        for (int x = 0; (float) x < Width / 8f; x++) {
                            for (int y = 0; (float) y < Height / 8f; y++) {
                                if ((x + y) % 2 == particleAt) {
                                    SceneAs<Level>().ParticlesBG.Emit(SwitchGate.P_Behind, Position + new Vector2(x * 8, y * 8) + Calc.Random.Range(Vector2.One * 2f, Vector2.One * 6f));
                                }
                            }
                        }
                    }
                };
                tween.OnComplete = (t) => { waiting = false; };
                Add(tween);

                // wait for the move to be done.
                while (waiting) {
                    if (cancelMoving) {
                        tween.Stop();
                        Remove(tween);
                        moving = false;
                        yield break;
                    }
                    yield return null;
                }
                Remove(tween);


                bool wasCollidable = Collidable;
                // collide dust particles on the left
                if (node.X <= start.X) {
                    Vector2 add = new Vector2(0f, 2f);
                    for (int tileY = 0; tileY < Height / 8f; tileY++) {
                        Vector2 collideAt = new Vector2(Left - 1f, Top + 4f + (tileY * 8));
                        Vector2 noCollideAt = collideAt + Vector2.UnitX;
                        if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt)) {
                            SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, (float) Math.PI);
                            SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, (float) Math.PI);
                        }
                    }
                }

                // collide dust particles on the rigth
                if (node.X >= start.X) {
                    Vector2 add = new Vector2(0f, 2f);
                    for (int tileY = 0; tileY < Height / 8f; tileY++) {
                        Vector2 collideAt = new Vector2(Right + 1f, Top + 4f + (tileY * 8));
                        Vector2 noCollideAt = collideAt - Vector2.UnitX * 2f;
                        if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt)) {
                            SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, 0f);
                            SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, 0f);
                        }
                    }
                }

                // collide dust particles on the top
                if (node.Y <= start.Y) {
                    Vector2 add = new Vector2(2f, 0f);
                    for (int tileX = 0; tileX < Width / 8f; tileX++) {
                        Vector2 collideAt = new Vector2(Left + 4f + (tileX * 8), Top - 1f);
                        Vector2 noCollideAt = collideAt + Vector2.UnitY;
                        if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt)) {
                            SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, -(float) Math.PI / 2f);
                            SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, -(float) Math.PI / 2f);
                        }
                    }
                }

                // collide dust particles on the bottom
                if (node.Y >= start.Y) {
                    Vector2 add = new Vector2(2f, 0f);
                    for (int tileX = 0; tileX < Width / 8f; tileX++) {
                        Vector2 collideAt = new Vector2(Left + 4f + (tileX * 8), Bottom + 1f);
                        Vector2 noCollideAt = collideAt - Vector2.UnitY * 2f;
                        if (Scene.CollideCheck<Solid>(collideAt) && !Scene.CollideCheck<Solid>(noCollideAt)) {
                            SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt + add, (float) Math.PI / 2f);
                            SceneAs<Level>().ParticlesFG.Emit(SwitchGate.P_Dust, collideAt - add, (float) Math.PI / 2f);
                        }
                    }
                }
                Collidable = wasCollidable;

                Audio.Play("event:/game/general/touchswitch_gate_finish", Position);

                // shake after arriving at destination
                StartShaking(0.2f);
                while (icon.Rate > 0f && !cancelMoving) {
                    icon.Color = Color.Lerp(activeColor, nodeIndex > 0 ? finishColor : inactiveColor, 1f - icon.Rate);
                    icon.Rate -= Engine.DeltaTime * 4f;
                    yield return null;
                }
                if (cancelMoving) {
                    moving = false;
                    yield break;
                }

                icon.Rate = 0f;
                icon.SetAnimationFrame(0);

                // animate the icon with particles
                wiggler.Start();
                wasCollidable = Collidable;
                Collidable = false;
                if (!Scene.CollideCheck<Solid>(Center)) {
                    for (int i = 0; i < 32; i++) {
                        float angle = Calc.Random.NextFloat((float) Math.PI * 2f);
                        SceneAs<Level>().ParticlesFG.Emit(TouchSwitch.P_Fire, Position + iconOffset + Calc.AngleToVector(angle, 4f), angle);
                    }
                }
                Collidable = wasCollidable;
            } else {
                // we are "moving" without changing positions: just animate the icon.
                icon.Rate = 1f;
                while (icon.Rate > 0f && !cancelMoving) {
                    var lastColor = icon.Color;
                    icon.Color = Color.Lerp(lastColor, nodeIndex > 0 ? finishColor : inactiveColor, 1f - icon.Rate);
                    icon.Rate -= Engine.DeltaTime * 4f;
                    yield return null;
                }
                icon.Color = finishColor;
                icon.Rate = 0f;
                icon.SetAnimationFrame(0);
            }

            moving = false;
        }
    }
}
