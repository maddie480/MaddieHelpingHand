using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// A touch switch triggering an arbitrary session flag.
    /// 
    /// Attributes:
    /// - flag: the session flag this touch switch sets. Must be the same across the whole touch switch group.
    /// - icon: the name of the icon for the touch switch (relative to objects/MaxHelpingHand/flagTouchSwitch) or "vanilla" for the default one.
    /// - persistent: enable to have the switch stay active even when you die / change rooms.
    /// - inactiveColor / activeColor / finishColor: custom colors for the touch switch.
    /// </summary>
    [CustomEntity("MaxHelpingHand/FlagTouchSwitch")]
    [Tracked]
    class FlagTouchSwitch : Entity {
        private static FieldInfo seekerPushRadius = typeof(Seeker).GetField("pushRadius", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo seekerPhysicsHitbox = typeof(Seeker).GetField("physicsHitbox", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo pufferPushRadius = typeof(Puffer).GetField("pushRadius", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Load() {
            On.Celeste.Seeker.RegenerateCoroutine += onSeekerRegenerateCoroutine;
            On.Celeste.Puffer.Explode += onPufferExplode;
        }

        public static void Unload() {
            On.Celeste.Seeker.RegenerateCoroutine -= onSeekerRegenerateCoroutine;
            On.Celeste.Puffer.Explode -= onPufferExplode;
        }

        private static IEnumerator onSeekerRegenerateCoroutine(On.Celeste.Seeker.orig_RegenerateCoroutine orig, Seeker self) {
            IEnumerator origEnum = orig(self);
            while (origEnum.MoveNext()) {
                yield return origEnum.Current;
            }

            // make the seeker check for flag touch switches as well.
            self.Collider = (Collider) seekerPushRadius.GetValue(self);
            turnOnTouchSwitchesCollidingWith(self);
            self.Collider = (Collider) seekerPhysicsHitbox.GetValue(self);
        }

        private static void onPufferExplode(On.Celeste.Puffer.orig_Explode orig, Puffer self) {
            orig(self);

            // make the puffer check for flag touch switches as well.
            Collider oldCollider = self.Collider;
            self.Collider = (Collider) pufferPushRadius.GetValue(self);
            turnOnTouchSwitchesCollidingWith(self);
            self.Collider = oldCollider;
        }

        private static void turnOnTouchSwitchesCollidingWith(Entity self) {
            foreach (FlagTouchSwitch touchSwitch in self.Scene.Tracker.GetEntities<FlagTouchSwitch>()) {
                if (self.CollideCheck(touchSwitch)) {
                    touchSwitch.turnOn();
                }
            }
        }

        private ParticleType P_RecoloredFire;

        private int id;
        private string flag;

        // contains all the touch switches in the room
        private List<FlagTouchSwitch> allTouchSwitchesInRoom;

        private bool activated = false;
        private bool finished = false;

        private SoundSource touchSfx;

        private MTexture border = GFX.Game["objects/touchswitch/container"];

        private Sprite icon;
        private bool persistent;

        private Color inactiveColor;
        private Color activeColor;
        private Color finishColor;

        private float ease;

        private Wiggler wiggler;

        private Vector2 pulse = Vector2.One;

        private float timer = 0f;

        private BloomPoint bloom;

        private Level level => (Level) Scene;

        public FlagTouchSwitch(EntityData data, Vector2 offset)
            : base(data.Position + offset) {

            Depth = 2000;

            id = data.ID;
            flag = data.Attr("flag");
            persistent = data.Bool("persistent", false);

            inactiveColor = Calc.HexToColor(data.Attr("inactiveColor", "5FCDE4"));
            activeColor = Calc.HexToColor(data.Attr("activeColor", "FFFFFF"));
            finishColor = Calc.HexToColor(data.Attr("finishColor", "F141DF"));

            P_RecoloredFire = new ParticleType(TouchSwitch.P_Fire) {
                Color = finishColor
            };

            // set up collision
            Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(onPlayer, null, new Hitbox(30f, 30f, -15f, -15f)));
            Add(new HoldableCollider(onHoldable, new Hitbox(20f, 20f, -10f, -10f)));
            Add(new SeekerCollider(onSeeker, new Hitbox(24f, 24f, -12f, -12f)));

            // set up the icon
            string iconAttribute = data.Attr("icon", "vanilla");
            icon = new Sprite(GFX.Game, iconAttribute == "vanilla" ? "objects/touchswitch/icon" : $"objects/MaxHelpingHand/flagTouchSwitch/{iconAttribute}/icon");
            Add(icon);
            icon.Add("idle", "", 0f, default(int));
            icon.Add("spin", "", 0.1f, new Chooser<string>("spin", 1f), 0, 1, 2, 3, 4, 5);
            icon.Play("spin");
            icon.Color = inactiveColor;
            icon.CenterOrigin();

            Add(bloom = new BloomPoint(0f, 16f));
            bloom.Alpha = 0f;

            Add(wiggler = Wiggler.Create(0.5f, 4f, v => {
                pulse = Vector2.One * (1f + v * 0.25f);
            }));

            Add(new VertexLight(Color.White, 0.8f, 16, 32));
            Add(touchSfx = new SoundSource());
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (level.Session.GetFlag(flag)) {
                // start directly finished, since the session flag is already set.
                activated = true;
                finished = true;

                icon.Rate = 0.1f;
                icon.Play("idle");
                icon.Color = finishColor;
                ease = 1f;
            } else if (level.Session.GetFlag(flag + "_switch" + id)) {
                // only that switch is activated, not the whole group.
                activated = true;

                icon.Rate = 4f;
                icon.Color = activeColor;
                ease = 1f;
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // look around for other touch switches that belong to the same group (same flag).
            allTouchSwitchesInRoom = Scene.Tracker.GetEntities<FlagTouchSwitch>()
                .FindAll(touchSwitch => (touchSwitch as FlagTouchSwitch)?.flag == flag).OfType<FlagTouchSwitch>().ToList();
        }

        private void onPlayer(Player player) {
            turnOn();
        }

        private void onHoldable(Holdable h) {
            turnOn();
        }

        private void onSeeker(Seeker seeker) {
            if (SceneAs<Level>().InsideCamera(Position, 10f)) {
                turnOn();
            }
        }

        private void turnOn() {
            if (!activated) {
                touchSfx.Play("event:/game/general/touchswitch_any");

                activated = true;

                // animation
                wiggler.Start();
                for (int i = 0; i < 32; i++) {
                    float num = Calc.Random.NextFloat((float) Math.PI * 2f);
                    level.Particles.Emit(TouchSwitch.P_FireWhite, Position + Calc.AngleToVector(num, 6f), num);
                }
                icon.Rate = 4f;

                if (persistent) {
                    // this switch is persistent. save its activation in the session.
                    level.Session.SetFlag(flag + "_switch" + id, true);
                }

                if (MaxHelpingHandMapDataProcessor.FlagTouchSwitches[level.Session.Area.ID][(int) level.Session.Area.Mode][flag]
                    .All(touchSwitchID => touchSwitchID.Level == level.Session.Level || level.Session.GetFlag(flag + "_switch" + touchSwitchID.ID))
                    && allTouchSwitchesInRoom.All(touchSwitch => touchSwitch.activated)) {

                    // all switches in the room are enabled, and all session flags for switches outside the room are enabled.
                    // so, the group is complete.

                    foreach (FlagTouchSwitch touchSwitch in allTouchSwitchesInRoom) {
                        touchSwitch.finish();
                    }

                    SoundEmitter.Play("event:/game/general/touchswitch_last_oneshot");
                    Add(new SoundSource("event:/game/general/touchswitch_last_cutoff"));

                    // trigger associated switch gate(s).
                    foreach (FlagSwitchGate switchGate in Scene.Tracker.GetEntities<FlagSwitchGate>().OfType<FlagSwitchGate>()) {
                        if (switchGate.Flag == flag) {
                            switchGate.Trigger();
                        }
                    }

                    // set flags for switch gates.
                    bool allGatesTriggered = true;
                    if (MaxHelpingHandMapDataProcessor.FlagSwitchGates[level.Session.Area.ID][(int) level.Session.Area.Mode].ContainsKey(flag)) {
                        Dictionary<EntityID, bool> allGates = MaxHelpingHandMapDataProcessor.FlagSwitchGates[level.Session.Area.ID][(int) level.Session.Area.Mode][flag];
                        foreach (KeyValuePair<EntityID, bool> gate in allGates) {
                            if (gate.Value) {
                                // the gate is persistent; set the flag
                                level.Session.SetFlag(flag + "_gate" + gate.Key.ID);
                            } else {
                                // one of the gates is not persistent, so the touch switches shouldn't be forced to persist.
                                allGatesTriggered = false;
                            }
                        }
                    }

                    // if all the switches OR all the gates are persistent, the flag it's setting is persistent.
                    if (allTouchSwitchesInRoom.All(touchSwitch => touchSwitch.persistent) || allGatesTriggered) {
                        level.Session.SetFlag(flag, true);
                    }
                }
            }
        }

        private void finish() {
            finished = true;
            ease = 0f;
        }

        public override void Update() {
            timer += Engine.DeltaTime * 8f;
            ease = Calc.Approach(ease, (finished || activated) ? 1f : 0f, Engine.DeltaTime * 2f);

            icon.Color = Color.Lerp(inactiveColor, finished ? finishColor : activeColor, ease);
            icon.Color *= 0.5f + ((float) Math.Sin(timer) + 1f) / 2f * (1f - ease) * 0.5f + 0.5f * ease;

            bloom.Alpha = ease;
            if (finished) {
                if (icon.Rate > 0.1f) {
                    icon.Rate -= 2f * Engine.DeltaTime;
                    if (icon.Rate <= 0.1f) {
                        icon.Rate = 0.1f;
                        wiggler.Start();
                        icon.Play("idle");
                        level.Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
                    }
                } else if (Scene.OnInterval(0.03f)) {
                    Vector2 position = Position + new Vector2(0f, 1f) + Calc.AngleToVector(Calc.Random.NextAngle(), 5f);
                    level.ParticlesBG.Emit(P_RecoloredFire, position);
                }
            }

            base.Update();
        }

        public override void Render() {
            border.DrawCentered(Position + new Vector2(0f, -1f), Color.Black);
            border.DrawCentered(Position, icon.Color, pulse);
            base.Render();
        }
    }
}