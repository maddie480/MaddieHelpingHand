using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// Almost a copypaste of the vanilla SandwichLava class, but:
    /// - plays nicely with vertical rooms
    /// - can be made one-way
    /// - has customizable speed and gap.
    /// </summary>
    [CustomEntity("MaxHelpingHand/CustomSandwichLava")]
    [Tracked]
    public class CustomSandwichLava : Entity {
        private static FieldInfo lavaBlockerTriggerEnabled = typeof(LavaBlockerTrigger).GetField("enabled", BindingFlags.NonPublic | BindingFlags.Instance);

        // if the player collides with one of those, lava should be forced into waiting.
        private List<LavaBlockerTrigger> lavaBlockerTriggers;

        public enum DirectionMode {
            AlwaysUp, AlwaysDown, CoreModeBased
        };

        private const float TopOffset = -160f;

        // parameters
        private float startX;
        public DirectionMode Direction;
        public float Speed;
        // the extra pixels each side has to be shifted towards each other compared to vanilla
        // to comply with the "sandwichGap" setting.
        private float sandwichDisplacement;
        private Color[] hot = new Color[3];
        private Color[] cold = new Color[3];

        // state
        private bool iceMode;
        private float lerp;
        public bool Waiting;
        private bool leaving = false;
        private float delay = 0f;
        private bool persistent;
        private bool entering = true;

        // during a transition, those hold the Y positions of lava parts from the beginning of the transition.
        private float transitionStartY;
        private float transitionStartTopRectY;
        private float transitionStartBottomRectY;

        private LavaRect bottomRect;
        private LavaRect topRect;

        private SoundSource loopSfx;

        // in vanilla, this is SceneAs<Level>().Bounds.Bottom. This is bad with vertical rooms.
        private float centerY => SceneAs<Level>().Camera.Bottom - 10f;

        public CustomSandwichLava(EntityData data, Vector2 offset) {
            startX = data.Position.X + offset.X;
            Direction = data.Enum("direction", DirectionMode.CoreModeBased);
            Speed = data.Float("speed", 20f);

            hot[0] = Calc.HexToColor(data.Attr("hotSurfaceColor", "ff8933"));
            hot[1] = Calc.HexToColor(data.Attr("hotEdgeColor", "f25e29"));
            hot[2] = Calc.HexToColor(data.Attr("hotCenterColor", "d01c01"));
            cold[0] = Calc.HexToColor(data.Attr("coldSurfaceColor", "33ffe7"));
            cold[1] = Calc.HexToColor(data.Attr("coldEdgeColor", "4ca2eb"));
            cold[2] = Calc.HexToColor(data.Attr("coldCenterColor", "0151d0"));

            // vanilla is 160. so, setting sandwichGap to 120 requires each side to be shifted by 20 pixels towards the other ((160 - 120) / 2).
            sandwichDisplacement = (160f - data.Float("sandwichGap", 160f)) / 2;

            Depth = -1000000;
            Collider = new ColliderList(new Hitbox(340f, 120f, 0f, -sandwichDisplacement), new Hitbox(340f, 120f, 0f, -280f + sandwichDisplacement));
            Visible = false;

            Add(loopSfx = new SoundSource());
            Add(new PlayerCollider(OnPlayer));
            Add(new CoreModeListener(OnChangeMode));

            Add(bottomRect = new LavaRect(400f, 200f, 4));
            bottomRect.Position = new Vector2(-40f, 0f);
            bottomRect.OnlyMode = LavaRect.OnlyModes.OnlyTop;
            bottomRect.SmallWaveAmplitude = 2f;

            Add(topRect = new LavaRect(400f, 200f, 4));
            topRect.Position = new Vector2(-40f, -360f);
            topRect.OnlyMode = LavaRect.OnlyModes.OnlyBottom;
            topRect.SmallWaveAmplitude = 2f;
            topRect.BigWaveAmplitude = (bottomRect.BigWaveAmplitude = 2f);
            topRect.CurveAmplitude = (bottomRect.CurveAmplitude = 4f);

            Add(new TransitionListener {
                OnOutBegin = () => {
                    // save the Y positions
                    transitionStartY = Y;
                    transitionStartTopRectY = topRect.Position.Y;
                    transitionStartBottomRectY = bottomRect.Position.Y;

                    if (persistent && Scene != null && Scene.Entities.FindAll<CustomSandwichLava>().Count <= 1) {
                        // no lava in the next room: leave
                        Leave();
                    } else {
                        // look up for all lava blocker triggers in the next room.
                        lavaBlockerTriggers = Scene.Entities.OfType<LavaBlockerTrigger>().ToList();
                    }
                },
                OnOut = progress => {
                    if (Scene != null) {
                        X = (Scene as Level).Camera.X;
                        if (!leaving) {
                            // make the lava elements transition smoothly to their expected positions.
                            Y = MathHelper.Lerp(transitionStartY, centerY, progress);
                            topRect.Position.Y = MathHelper.Lerp(transitionStartTopRectY, TopOffset - topRect.Height + sandwichDisplacement, progress);
                            bottomRect.Position.Y = MathHelper.Lerp(transitionStartBottomRectY, -sandwichDisplacement, progress);
                        }
                    }

                    if ((progress > 0.95f) && leaving) {
                        // lava is leaving, transition is over soon => remove it
                        RemoveSelf();
                    }
                },
                OnInEnd = () => {
                    if (entering) {
                        // transition is over. grab the camera position now since it's done moving.
                        Y = centerY;
                        entering = false;
                    }
                }
            });
        }

        public override void Added(Scene scene) {
            base.Added(scene);
            X = SceneAs<Level>().Bounds.Left - 10;
            Y = centerY;
            iceMode = (SceneAs<Level>().Session.CoreMode == Session.CoreModes.Cold);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null && (entity.JustRespawned || entity.X < startX)) {
                Waiting = true;
            }

            List<CustomSandwichLava> existingLavas = Scene.Entities.FindAll<CustomSandwichLava>();

            bool removedSelf = false;
            if (!persistent && existingLavas.Count >= 2) {
                // another lava entity already exists.
                CustomSandwichLava sandwichLava = (existingLavas[0] == this) ? existingLavas[1] : existingLavas[0];
                if (!sandwichLava.leaving) {
                    // transfer new settings to the existing lava.
                    sandwichLava.startX = startX;
                    sandwichLava.Waiting = true;
                    sandwichLava.Direction = Direction;
                    sandwichLava.Speed = Speed;
                    sandwichLava.sandwichDisplacement = sandwichDisplacement;
                    sandwichLava.Collider = Collider;
                    entering = false;
                    RemoveSelf();
                    removedSelf = true;
                }
            }

            if (!removedSelf) {
                // turn the lava persistent, so that it animates during room transitions
                // and deals smoothly with successive rooms with sandwich lava.
                persistent = true;
                Tag = Tags.Persistent;

                if ((scene as Level).LastIntroType != Player.IntroTypes.Respawn) {
                    // throw the two lava parts off-screen.
                    topRect.Position.Y -= 60f + sandwichDisplacement;
                    bottomRect.Position.Y += 60f + sandwichDisplacement;
                } else {
                    Visible = true;
                }

                loopSfx.Play("event:/game/09_core/rising_threat", "room_state", iceMode ? 1 : 0);
                loopSfx.Position = new Vector2(Width / 2f, 0f);

                // look up for all lava blocker triggers in the room.
                lavaBlockerTriggers = scene.Entities.OfType<LavaBlockerTrigger>().ToList();
            }
        }

        private void OnChangeMode(Session.CoreModes mode) {
            iceMode = (mode == Session.CoreModes.Cold);
            loopSfx.Param("room_state", iceMode ? 1 : 0);
        }

        private void OnPlayer(Player player) {
            if (Waiting) {
                return;
            }
            if (SaveData.Instance.Assists.Invincible) {
                if (delay <= 0f) {
                    int direction = (player.Y > Y + bottomRect.Position.Y - 32f) ? 1 : -1;

                    float from = Y;
                    float to = Y + (direction * 48);
                    player.Speed.Y = -direction * 200;

                    if (direction > 0) {
                        player.RefillDash();
                    }

                    Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.CubeOut, tween => {
                        Y = MathHelper.Lerp(from, to, tween.Eased);
                    });

                    delay = 0.5f;
                    loopSfx.Param("rising", 0f);
                    Audio.Play("event:/game/general/assist_screenbottom", player.Position);
                }
            } else {
                player.Die(-Vector2.UnitY);
            }
        }

        public void Leave() {
            AddTag(Tags.TransitionUpdate);
            leaving = true;
            Collidable = false;
            Alarm.Set(this, 2f, delegate {
                RemoveSelf();
            });
        }

        public override void Update() {
            if (entering) {
                // set the Y position again on the first Update call, since the camera is finished being set up.
                Y = centerY;
                entering = false;
            }

            Level level = Scene as Level;
            X = level.Camera.X;
            delay -= Engine.DeltaTime;
            base.Update();
            Visible = true;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                LavaBlockerTrigger collidedTrigger = lavaBlockerTriggers.Find(trigger => player.CollideCheck(trigger));

                if (collidedTrigger != null && (bool) lavaBlockerTriggerEnabled.GetValue(collidedTrigger)) {
                    // player is in a lava blocker trigger and it is enabled; block the lava.
                    Waiting = true;
                }
            }

            if (Waiting) {
                Y = Calc.Approach(Y, centerY, 128f * Engine.DeltaTime);

                loopSfx.Param("rising", 0f);
                if (player != null && player.X >= startX && !player.JustRespawned && player.StateMachine.State != 11) {
                    Waiting = false;
                }
            } else if (!leaving && delay <= 0f) {
                loopSfx.Param("rising", 1f);
                if (Direction == DirectionMode.AlwaysDown || (Direction == DirectionMode.CoreModeBased && iceMode)) {
                    Y += Speed * Engine.DeltaTime;
                } else {
                    Y -= Speed * Engine.DeltaTime;
                }
            }

            topRect.Position.Y = Calc.Approach(topRect.Position.Y, TopOffset - topRect.Height + (leaving ? (-512) : sandwichDisplacement), (leaving ? 256 : 64) * Engine.DeltaTime);
            bottomRect.Position.Y = Calc.Approach(bottomRect.Position.Y, leaving ? 512 : -sandwichDisplacement, (leaving ? 256 : 64) * Engine.DeltaTime);

            lerp = Calc.Approach(lerp, iceMode ? 1 : 0, Engine.DeltaTime * 4f);

            bottomRect.SurfaceColor = Color.Lerp(hot[0], cold[0], lerp);
            bottomRect.EdgeColor = Color.Lerp(hot[1], cold[1], lerp);
            bottomRect.CenterColor = Color.Lerp(hot[2], cold[2], lerp);
            bottomRect.Spikey = lerp * 5f;
            bottomRect.UpdateMultiplier = (1f - lerp) * 2f;
            bottomRect.Fade = (iceMode ? 128 : 32);

            topRect.SurfaceColor = bottomRect.SurfaceColor;
            topRect.EdgeColor = bottomRect.EdgeColor;
            topRect.CenterColor = bottomRect.CenterColor;
            topRect.Spikey = bottomRect.Spikey;
            topRect.UpdateMultiplier = bottomRect.UpdateMultiplier;
            topRect.Fade = bottomRect.Fade;
        }
    }
}