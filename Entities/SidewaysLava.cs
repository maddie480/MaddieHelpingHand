using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Celeste.Mod.MaxHelpingHand.Triggers;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// Mashup of vanilla RisingLava and SandwichLava, allowing for lava coming from the sides instead of from the top / bottom.
    ///
    /// Attributes:
    /// - intro: if true, lava will be invisible until the player moves
    /// - lavaMode: allows picking the lava direction (left to right, right to left, or sandwich)
    /// - speedMultiplier: multiplies the vanilla speed for lava
    /// </summary>
    [CustomEntity("MaxHelpingHand/SidewaysLava")]
    public class SidewaysLava : Entity {
        // Everest stuff isn't publicized
        private static FieldInfo lavaBlockerTriggerEnabled = typeof(LavaBlockerTrigger).GetField("enabled", BindingFlags.NonPublic | BindingFlags.Instance);

        private enum LavaMode {
            LeftToRight, RightToLeft, Sandwich
        }

        private const float Speed = 30f;

        // if the player collides with one of those, lava should be forced into waiting.
        private List<LavaBlockerTrigger> lavaBlockerTriggers;

        // atrributes
        private bool intro;
        private LavaMode lavaMode;
        private float speedMultiplier;
        private Color[] hotColors, coldColors;
        private string sound;
        private Session.CoreModes forcedCoreMode;
        private string flag;

        // state keeping
        private bool iceMode;
        private bool waiting;
        private float delay = 0f;
        private float lerp;

        // sandwich-specific stuff
        private bool sandwichLeaving = false;
        private float sandwichTransitionStartX = 0;
        private bool sandwichHasToSetPosition = false;
        private bool sandwichTransferred = false;

        private SidewaysLavaRect leftRect;
        private SidewaysLavaRect rightRect;

        private int lastCameraWidth;
        private int lastCameraHeight;

        private SoundSource loopSfx;

        public SidewaysLava(bool intro, string lavaMode, float speedMultiplier) : this(new EntityData() {
            Values = new Dictionary<string, object>() {
                { "intro", intro }, { "lavaMode", lavaMode }, { "speedMultiplier", speedMultiplier }
            }
        }, Vector2.Zero) { }

        public SidewaysLava(EntityData data, Vector2 offset) {
            intro = data.Bool("intro", false);
            lavaMode = data.Enum("lavaMode", LavaMode.LeftToRight);
            speedMultiplier = data.Float("speedMultiplier", 1f);
            sound = data.Attr("sound", defaultValue: "event:/game/09_core/rising_threat");
            forcedCoreMode = data.Enum("forceCoreMode", defaultValue: Session.CoreModes.None);
            flag = data.Attr("flag");

            hotColors = new Color[3];
            hotColors[0] = Calc.HexToColor(data.Attr("hotSurfaceColor", "ff8933"));
            hotColors[1] = Calc.HexToColor(data.Attr("hotEdgeColor", "f25e29"));
            hotColors[2] = Calc.HexToColor(data.Attr("hotCenterColor", "d01c01"));

            coldColors = new Color[3];
            coldColors[0] = Calc.HexToColor(data.Attr("coldSurfaceColor", "33ffe7"));
            coldColors[1] = Calc.HexToColor(data.Attr("coldEdgeColor", "4ca2eb"));
            coldColors[2] = Calc.HexToColor(data.Attr("coldCenterColor", "0151d0"));

            Depth = -1000000;

            setupCollider();

            Visible = false;
            Add(new PlayerCollider(OnPlayer));
            Add(new CoreModeListener(OnChangeMode));
            Add(loopSfx = new SoundSource());

            // lava can travel at up to 40 px/s * speedMultiplier, and we want it to extend enough so that you don't see it scrolling past the screen.
            float lavaWidth = MaxHelpingHandModule.CameraWidth + speedMultiplier * 80f;

            if (lavaMode != LavaMode.RightToLeft) {
                // add the left lava rect, just off-screen (it is 340px wide)
                Add(leftRect = new SidewaysLavaRect(lavaWidth, MaxHelpingHandModule.CameraHeight + 20f, 4, SidewaysLavaRect.OnlyModes.OnlyLeft));
                leftRect.Position = new Vector2(-lavaWidth, 0f);
                leftRect.SmallWaveAmplitude = 2f;
            }
            if (lavaMode != LavaMode.LeftToRight) {
                // add the right lava rect, just off-screen (the screen is 320px wide)
                Add(rightRect = new SidewaysLavaRect(lavaWidth, MaxHelpingHandModule.CameraHeight + 20f, 4, SidewaysLavaRect.OnlyModes.OnlyRight));
                rightRect.Position = new Vector2(lavaMode == LavaMode.Sandwich ? MaxHelpingHandModule.CameraWidth - 40f : MaxHelpingHandModule.CameraWidth, 0f);
                rightRect.SmallWaveAmplitude = 2f;
            }

            if (lavaMode == LavaMode.Sandwich) {
                // listen to transitions since we need the sandwich lava to deal smoothly with them.
                Add(new TransitionListener {
                    OnOutBegin = () => {
                        sandwichTransitionStartX = X;
                        if (!sandwichTransferred) {
                            // the next screen has no sideways sandwich lava. so, just leave.
                            AddTag(Tags.TransitionUpdate);
                            sandwichLeaving = true;
                            Collidable = false;
                            Alarm.Set(this, 2f, () => RemoveSelf());
                        } else {
                            sandwichTransferred = false;

                            // look up for all lava blocker triggers in the next room.
                            lavaBlockerTriggers = Scene.Entities.OfType<LavaBlockerTrigger>().ToList();
                        }
                    },
                    OnOut = progress => {
                        if (Scene != null) {
                            Level level = Scene as Level;

                            // make sure the sandwich lava is following the transition.
                            Y = level.Camera.Y - 10f;

                            if (!sandwichLeaving) {
                                // make the lava smoothly go back to 20px on each side.
                                X = MathHelper.Lerp(sandwichTransitionStartX, level.Camera.Left + 20f, progress);
                            }
                        }
                        if (progress > 0.95f && sandwichLeaving) {
                            // destroy the lava, since transition is almost done.
                            RemoveSelf();
                        }
                    }
                });
            }

            lastCameraWidth = MaxHelpingHandModule.CameraWidth;
            lastCameraHeight = MaxHelpingHandModule.CameraHeight;
        }

        private void setupCollider() {
            if (lavaMode == LavaMode.LeftToRight) {
                // one hitbox on the left.
                Collider = new Hitbox(MaxHelpingHandModule.CameraWidth + 20f, MaxHelpingHandModule.CameraHeight + 20f, -MaxHelpingHandModule.CameraWidth - 20f);
            } else if (lavaMode == LavaMode.RightToLeft) {
                // one hitbox on the right.
                Collider = new Hitbox(MaxHelpingHandModule.CameraWidth + 20f, MaxHelpingHandModule.CameraHeight + 20f, MaxHelpingHandModule.CameraWidth);
            } else {
                // hitboxes on both sides, 280px apart.
                Collider = new ColliderList(new Hitbox(MaxHelpingHandModule.CameraWidth + 20f, MaxHelpingHandModule.CameraHeight + 20f, -MaxHelpingHandModule.CameraWidth - 20f), new Hitbox(MaxHelpingHandModule.CameraWidth + 20f, MaxHelpingHandModule.CameraHeight + 20f, MaxHelpingHandModule.CameraWidth - 40f));
            }
        }

        private void checkCameraDimensionChange() {
            if (MaxHelpingHandModule.CameraWidth == lastCameraWidth && MaxHelpingHandModule.CameraHeight == lastCameraHeight) {
                return;
            }

            // resize colliders
            setupCollider();

            // resize lava graphics
            float lavaWidth = MaxHelpingHandModule.CameraWidth + speedMultiplier * 80f;
            if (lavaMode != LavaMode.RightToLeft) {
                leftRect.Resize(lavaWidth, MaxHelpingHandModule.CameraHeight + 20f, leftRect.SurfaceStep);
                leftRect.Position = new Vector2(-lavaWidth, 0f);
            }
            if (lavaMode != LavaMode.LeftToRight) {
                rightRect.Resize(lavaWidth, MaxHelpingHandModule.CameraHeight + 20f, rightRect.SurfaceStep);
                rightRect.Position = new Vector2(lavaMode == LavaMode.Sandwich ? MaxHelpingHandModule.CameraWidth - 40f : MaxHelpingHandModule.CameraWidth, 0f);
            }

            if (lavaMode == LavaMode.RightToLeft || (lavaMode == LavaMode.Sandwich && iceMode)) {
                // if the hitbox just got nudged 5px to the right, nudge the entity 5px to the left to compensate
                X -= MaxHelpingHandModule.CameraWidth - lastCameraWidth;
            }
            // LeftToRight doesn't need this, since the player sees the right side, which is at 0.
            // Sandwich goes right to left when iceMode = true, so it gets the same treatment as RightToLeft in that case.

            lastCameraWidth = MaxHelpingHandModule.CameraWidth;
            lastCameraHeight = MaxHelpingHandModule.CameraHeight;
        }

        private static Color[] parseColors(string input) {
            string[] colorsAsStrings = input.Split(',');
            Color[] colors = new Color[colorsAsStrings.Length];
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = Calc.HexToColor(colorsAsStrings[i]);
            }
            return colors;
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            iceMode = (getActiveCoreMode(SceneAs<Level>().CoreMode) == Session.CoreModes.Cold);
            loopSfx.Play(sound, "room_state", iceMode ? 1 : 0);

            if (lavaMode == LavaMode.LeftToRight) {
                // make the lava off-screen by 16px.
                X = SceneAs<Level>().Bounds.Left - 16;
                // sound comes from the left side.
                loopSfx.Position = new Vector2(0f, Height / 2f);

            } else if (lavaMode == LavaMode.RightToLeft) {
                // same, except the lava is offset by 320px. That gives Right - 320 + 16.
                X = SceneAs<Level>().Bounds.Right - MaxHelpingHandModule.CameraWidth + 16;
                // sound comes from the right side.
                loopSfx.Position = new Vector2(MaxHelpingHandModule.CameraWidth, Height / 2f);

            } else {
                // the position should be set on the first Update call, in case the level starts with a room with lava in it
                // and the camera doesn't really exist yet.
                sandwichHasToSetPosition = true;
                // sound comes from the middle.
                loopSfx.Position = new Vector2(140f, Height / 2f);
            }

            Y = SceneAs<Level>().Bounds.Top - 10;

            if (lavaMode == LavaMode.Sandwich) {
                // check if another sandwich lava is already here.
                List<SidewaysLava> sandwichLavas = new List<SidewaysLava>(Scene.Entities.FindAll<SidewaysLava>()
                    .Where(lava => (lava as SidewaysLava).lavaMode == LavaMode.Sandwich));

                bool didRemoveSelf = false;
                if (sandwichLavas.Count >= 2) {
                    SidewaysLava otherLava = (sandwichLavas[0] == this) ? sandwichLavas[1] : sandwichLavas[0];
                    if (!otherLava.sandwichLeaving) {
                        // just let the existing lava do the job. transfer settings to it.
                        otherLava.speedMultiplier = speedMultiplier;
                        otherLava.sandwichTransferred = true;
                        RemoveSelf();
                        didRemoveSelf = true;
                    }
                }

                if (!didRemoveSelf) {
                    // we should make ourselves persistent to handle transitions smoothly.
                    Tag = Tags.Persistent;

                    if ((scene as Level).LastIntroType != Player.IntroTypes.Respawn) {
                        // both rects start from off-screen, and fade in.
                        leftRect.Position.X -= 60f;
                        rightRect.Position.X += 60f;
                    } else {
                        // start directly visible if we respawned in the room (likely from dying in it).
                        Visible = true;
                    }
                }
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            if (intro || (Scene.Tracker.GetEntity<Player>()?.JustRespawned ?? false)) {
                // wait for the player to move before starting.
                waiting = true;
            }

            if (intro) {
                Visible = true;
            }

            // look up for all lava blocker triggers in the room.
            lavaBlockerTriggers = scene.Entities.OfType<LavaBlockerTrigger>().ToList();
        }

        private void OnChangeMode(Session.CoreModes mode) {
            iceMode = (getActiveCoreMode(mode) == Session.CoreModes.Cold);
            loopSfx.Param("room_state", iceMode ? 1 : 0);
        }

        private Session.CoreModes getActiveCoreMode(Session.CoreModes actualCoreMode) {
            if (forcedCoreMode == Session.CoreModes.None) {
                return actualCoreMode;
            }
            return forcedCoreMode;
        }

        private void OnPlayer(Player player) {
            int direction; // 1 if right lava was hit, -1 is left lava was hit.
            if (lavaMode == LavaMode.LeftToRight) {
                direction = -1;
            } else if (lavaMode == LavaMode.RightToLeft) {
                direction = 1;
            } else {
                // determine which side was hit depending on the player position.
                direction = (player.X > X + rightRect.Position.X - 32f) ? 1 : -1;
            }

            if (SaveData.Instance.Assists.Invincible) {
                if (delay <= 0f) {
                    float from = X;
                    float to = X + 48f * direction;
                    player.Speed.X = -200f * direction;

                    player.RefillDash();
                    Tween.Set(this, Tween.TweenMode.Oneshot, 0.4f, Ease.CubeOut, delegate(Tween t) {
                        X = MathHelper.Lerp(from, to, t.Eased);
                    });
                    delay = 0.5f;
                    loopSfx.Param("rising", 0f);
                    Audio.Play("event:/game/general/assist_screenbottom", player.Position);
                }
            } else {
                player.Die(Vector2.UnitX * -direction);
            }
        }

        public override void Update() {
            checkCameraDimensionChange();

            if (sandwichHasToSetPosition) {
                sandwichHasToSetPosition = false;

                // should be 20px to the right, so that the right rect is at 300px and both rects have the same on-screen size (20px).
                X = SceneAs<Level>().Camera.Left + 20f;
            }

            delay -= Engine.DeltaTime;
            Y = SceneAs<Level>().Camera.Y - 10f;
            base.Update();
            Visible = true;

            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null) {
                LavaBlockerTrigger collidedTrigger = lavaBlockerTriggers.Find(trigger => player.CollideCheck(trigger));

                if (collidedTrigger != null && (bool) lavaBlockerTriggerEnabled.GetValue(collidedTrigger)) {
                    // player is in a lava blocker trigger and it is enabled; block the lava.
                    waiting = true;
                }
            }

            if (!string.IsNullOrEmpty(flag) && !SceneAs<Level>().Session.GetFlag(flag)) {
                waiting = true;
            }

            if (waiting) {
                loopSfx.Param("rising", 0f);

                if (player == null || !player.JustRespawned) {
                    waiting = false;
                } else {
                    // the sandwich lava fade in animation is not handled here.
                    if (lavaMode != LavaMode.Sandwich) {
                        float target;
                        if (lavaMode == LavaMode.LeftToRight) {
                            // stop 32px to the left of the player.
                            target = player.X - 32f;
                        } else {
                            // stop 32px to the right of the player. since lava is offset by 320px, that gives 320 - 32.
                            target = player.X - MaxHelpingHandModule.CameraWidth + 32;
                        }

                        if (!intro && player != null && player.JustRespawned && !player.CollideCheck<InstantLavaBlockerTrigger>()) {
                            X = Calc.Approach(X, target, 32f * speedMultiplier * Engine.DeltaTime);
                        }
                    }
                }
            } else {
                if (lavaMode != LavaMode.Sandwich) {
                    // this is the X position around which the speed factor will be set. At this position, speedFactor = 1.
                    float positionThreshold;
                    // the current lava position.
                    float currentPosition;
                    // the direction the lava moves at (1 = right, -1 = left).
                    int direction;

                    if (lavaMode == LavaMode.LeftToRight) {
                        positionThreshold = SceneAs<Level>().Camera.Left + 21f;
                        // if lava is too far away, drag it in.
                        if (Right < positionThreshold - 96f) {
                            Right = positionThreshold - 96f;
                        }
                        currentPosition = Right;
                        direction = 1;
                    } else {
                        positionThreshold = SceneAs<Level>().Camera.Right - 21f;
                        // if lava is too far away, drag it in.
                        if (Left > positionThreshold + 96f) {
                            Left = positionThreshold + 96f;
                        }

                        // note: positionThreshold and currentPosition are negative here because the direction is inversed.
                        positionThreshold *= -1;
                        currentPosition = -Left;
                        direction = -1;
                    }

                    // those constants are just pulled from vanilla * 320 / 180, in an attempt to scale it for horizontal movement.
                    float speedFactor = (currentPosition > positionThreshold) ?
                        Calc.ClampedMap(currentPosition - positionThreshold, 0f, 56f, 1f, 0.5f) :
                        Calc.ClampedMap(currentPosition - positionThreshold, 0f, 170f, 1f, 2f);

                    if (delay <= 0f) {
                        loopSfx.Param("rising", 1f);
                        X += Speed * speedFactor * speedMultiplier * direction * Engine.DeltaTime;
                    }
                } else {
                    // sandwich lava moves at a constant speed depending on core mode.
                    int direction = iceMode ? -1 : 1;
                    loopSfx.Param("rising", 1f);
                    X += 20f * speedMultiplier * direction * Engine.DeltaTime;
                }
            }

            // lerp both lava rects when changing core mode.
            lerp = Calc.Approach(lerp, iceMode ? 1 : 0, Engine.DeltaTime * 4f);

            if (leftRect != null) {
                leftRect.SurfaceColor = Color.Lerp(hotColors[0], coldColors[0], lerp);
                leftRect.EdgeColor = Color.Lerp(hotColors[1], coldColors[1], lerp);
                leftRect.CenterColor = Color.Lerp(hotColors[2], coldColors[2], lerp);
                leftRect.Spikey = lerp * 5f;
                leftRect.UpdateMultiplier = (1f - lerp) * 2f;
                leftRect.Fade = (iceMode ? 128 : 32);
            }

            if (rightRect != null) {
                rightRect.SurfaceColor = Color.Lerp(hotColors[0], coldColors[0], lerp);
                rightRect.EdgeColor = Color.Lerp(hotColors[1], coldColors[1], lerp);
                rightRect.CenterColor = Color.Lerp(hotColors[2], coldColors[2], lerp);
                rightRect.Spikey = lerp * 5f;
                rightRect.UpdateMultiplier = (1f - lerp) * 2f;
                rightRect.Fade = (iceMode ? 128 : 32);
            }

            if (lavaMode == LavaMode.Sandwich) {
                // move lava rects towards their intended positions: -340 (0 - its width) for the left rect, 280 for the right rect.
                // if leaving, move them away quickly instead.
                leftRect.Position.X = Calc.Approach(leftRect.Position.X, -leftRect.Width + (sandwichLeaving ? -512 : 0), (sandwichLeaving ? 512 : 64) * Engine.DeltaTime);
                rightRect.Position.X = Calc.Approach(rightRect.Position.X, MaxHelpingHandModule.CameraWidth - 40f + (sandwichLeaving ? 512 : 0), (sandwichLeaving ? 512 : 64) * Engine.DeltaTime);
            }
        }
    }
}
