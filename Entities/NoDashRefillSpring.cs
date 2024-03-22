using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using Celeste.Mod.MaxHelpingHand.Module;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /**
     * A spring that does not give a refill when used.
     * Downwards spring code is based on Frost Helper's custom spring by JaThePlayer:
     * https://github.com/JaThePlayer/FrostHelper/blob/master/Code/FrostHelper/Entities/VanillaExtended/CustomSpring.cs
     */
    [CustomEntity("MaxHelpingHand/NoDashRefillSpring", "MaxHelpingHand/NoDashRefillSpringLeft",
        "MaxHelpingHand/NoDashRefillSpringRight", "MaxHelpingHand/NoDashRefillSpringDown")]
    [Tracked(inherited: true)]
    public class NoDashRefillSpring : Spring {
        public new enum Orientations { Floor, WallLeft, WallRight, Ceiling }
        public new Orientations Orientation;

        private static MethodInfo pufferGotoHitSpeed = typeof(Puffer).GetMethod("GotoHitSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo pufferAlert = typeof(Puffer).GetMethod("Alert", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Load() {
            On.Celeste.LightingRenderer.BeforeRender += onLightingBeforeRender;

            On.Celeste.Puffer.HitSpring += onPufferHitSpring;
            On.Celeste.Glider.HitSpring += onJellyHitSpring;
            On.Celeste.TheoCrystal.HitSpring += onTheoHitSpring;
        }

        public static void Unload() {
            On.Celeste.LightingRenderer.BeforeRender -= onLightingBeforeRender;

            On.Celeste.Puffer.HitSpring -= onPufferHitSpring;
            On.Celeste.Glider.HitSpring -= onJellyHitSpring;
            On.Celeste.TheoCrystal.HitSpring -= onTheoHitSpring;
        }

        private static void onLightingBeforeRender(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene) {
            orig(self, scene);

            List<Entity> springs = scene.Tracker.GetEntities<NoDashRefillSpring>();

            if (springs.Count != 0) {
                Draw.SpriteBatch.GraphicsDevice.SetRenderTarget(GameplayBuffers.Light);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
                foreach (Entity entity in springs) {
                    if (entity is NoDashRefillSpring spring && spring.ignoreLighting) {
                        Sprite sprite = spring.Get<Sprite>();
                        string path = sprite.Texture.AtlasPath;
                        path = path.Substring(0, path.LastIndexOf('/')) + "/white_" + path.Substring(path.LastIndexOf('/') + 1);
                        GFX.Game[path].Draw(spring.Position + sprite.Position - (scene as Level).Camera.Position, sprite.Origin, Color.White, sprite.Scale, sprite.Rotation);
                    }
                }
                Draw.SpriteBatch.End();
            }
        }

        private static bool onPufferHitSpring(On.Celeste.Puffer.orig_HitSpring orig, Puffer self, Spring spring) {
            if (spring is NoDashRefillSpring noDashRefillSpring && noDashRefillSpring.Orientation == Orientations.Ceiling) {
                DynData<Puffer> selfData = new DynData<Puffer>(self);
                if (selfData.Get<Vector2>("hitSpeed").Y <= 0f) {
                    pufferGotoHitSpeed.Invoke(self, new object[] { 224f * Vector2.UnitY });
                    self.MoveTowardsX(spring.CenterX, 4f);
                    selfData.Get<Wiggler>("bounceWiggler").Start();
                    pufferAlert.Invoke(self, new object[] { true, false });
                    return true;
                }
                return false;
            }
            return orig(self, spring);
        }

        private static bool onJellyHitSpring(On.Celeste.Glider.orig_HitSpring orig, Glider self, Spring spring) {
            if (spring is NoDashRefillSpring noDashRefillSpring && noDashRefillSpring.Orientation == Orientations.Ceiling) {
                if (!self.Hold.IsHeld && self.Speed.Y <= 0f) {
                    DynData<Glider> selfData = new DynData<Glider>(self);
                    self.Speed.X *= 0.5f;
                    self.Speed.Y = -160f;
                    selfData["noGravityTimer"] = 0.15f;
                    selfData.Get<Wiggler>("wiggler").Start();
                    return true;
                }
                return false;
            }
            return orig(self, spring);
        }

        private static bool onTheoHitSpring(On.Celeste.TheoCrystal.orig_HitSpring orig, TheoCrystal self, Spring spring) {
            if (spring is NoDashRefillSpring noDashRefillSpring && noDashRefillSpring.Orientation == Orientations.Ceiling) {
                if (!self.Hold.IsHeld && self.Speed.Y <= 0f) {
                    self.Speed.X *= 0.5f;
                    self.Speed.Y = -160f;
                    new DynData<TheoCrystal>(self)["noGravityTimer"] = 0.15f;
                    return true;
                }
                return false;
            }
            return orig(self, spring);
        }

        private static MethodInfo bounceAnimate = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.NonPublic | BindingFlags.Instance);
        private static object[] noParams = new object[0];

        private readonly bool ignoreLighting;
        private readonly bool refillStamina;

        private float inactiveTimer;

        public NoDashRefillSpring(EntityData data, Vector2 offset)
            : base(data.Position + offset, GetVanillaOrientationFromName(data.Name), data.Bool("playerCanUse", true)) {

            Orientation = GetOrientationFromName(data.Name);

            ignoreLighting = data.Bool("ignoreLighting", defaultValue: false);
            refillStamina = data.Bool("refillStamina", defaultValue: true);

            DynData<Spring> selfSpring = new DynData<Spring>(this);

            // remove the vanilla player collider. this is the one thing we want to mod here.
            foreach (Component component in this) {
                if (component.GetType() == typeof(PlayerCollider)) {
                    Remove(component);
                    break;
                }
            }

            // replace it with our own collider.
            if (data.Bool("playerCanUse", true)) {
                Add(new PlayerCollider(OnCollide));
            }

            // replace the vanilla sprite with our custom one.
            Sprite sprite = selfSpring.Get<Sprite>("sprite");
            sprite.Reset(GFX.Game, data.Attr("spriteDirectory", "objects/MaxHelpingHand/noDashRefillSpring") + "/");
            sprite.Add("idle", "", 0f, default(int));
            sprite.Add("bounce", "", 0.07f, "idle", 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 4, 5);
            sprite.Add("disabled", "white", 0.07f);
            sprite.Play("idle");
            sprite.Origin.X = sprite.Width / 2f;
            sprite.Origin.Y = sprite.Height;

            // handle the Ceiling orientation, since vanilla does not handle it.
            if (Orientation == Orientations.Ceiling) {
                Collider = new Hitbox(16f, 6f, -8f);
                Get<PufferCollider>().Collider = new Hitbox(16f, 10f, -8f, -4f);
                Get<Sprite>().Rotation = (float) Math.PI;
                Get<StaticMover>().SolidChecker = (Solid s) => CollideCheck(s, Position - Vector2.UnitY);
                Get<StaticMover>().JumpThruChecker = (JumpThru jt) => CollideCheck(jt, Position - Vector2.UnitY);
            }
        }

        private static Orientations GetOrientationFromName(string name) {
            switch (name) {
                case "MaxHelpingHand/NoDashRefillSpring":
                case "MaxHelpingHand/CustomDashRefillSpring":
                    return Orientations.Floor;
                case "MaxHelpingHand/NoDashRefillSpringRight":
                case "MaxHelpingHand/CustomDashRefillSpringRight":
                    return Orientations.WallRight;
                case "MaxHelpingHand/NoDashRefillSpringLeft":
                case "MaxHelpingHand/CustomDashRefillSpringLeft":
                    return Orientations.WallLeft;
                case "MaxHelpingHand/NoDashRefillSpringDown":
                case "MaxHelpingHand/CustomDashRefillSpringDown":
                    return Orientations.Ceiling;
                default:
                    throw new Exception("Custom Dash Refill Spring name doesn't correlate to a valid Orientation!");
            }
        }

        private static Spring.Orientations GetVanillaOrientationFromName(string name) {
            Orientations orientation = GetOrientationFromName(name);
            if (orientation == Orientations.Ceiling) {
                // doesn't exist in vanilla, use Floor as a placeholder to avoid a crash
                return Spring.Orientations.Floor;
            }
            return (Spring.Orientations) orientation;
        }

        public override void Update() {
            base.Update();
            if (inactiveTimer > 0f) inactiveTimer -= Engine.DeltaTime;
        }

        private void OnCollide(Player player) {
            if (player.StateMachine.State == 9) {
                return;
            }

            // Save dash and stamina count. Dashes / stamina are reloaded by SideBounce and SuperBounce.
            int originalDashCount = player.Dashes;
            float originalStamina = player.Stamina;

            bool bounced = false;
            bool inverted = GravityHelperImports.IsPlayerInverted();
            float realY = inverted ? -player.Speed.Y : player.Speed.Y;

            if (Orientation == Orientations.Floor) {
                if (realY >= 0f) {
                    if (inverted && inactiveTimer <= 0f) {
                        InvertedSuperBounce(player, Top - player.Height);
                        inactiveTimer = 0.1f;
                        bounced = true;
                    } else if (!inverted) {
                        player.SuperBounce(Top);
                        bounced = true;
                    }
                }
            } else if (Orientation == Orientations.WallLeft) {
                if (player.SideBounce(1, Right, CenterY)) {
                    bounced = true;
                }
            } else if (Orientation == Orientations.WallRight) {
                if (player.SideBounce(-1, Left, CenterY)) {
                    bounced = true;
                }
            } else if (Orientation == Orientations.Ceiling) {
                if (realY <= 0f) {
                    if (!inverted && inactiveTimer <= 0f) {
                        InvertedSuperBounce(player, Bottom + player.Height);
                        inactiveTimer = 0.1f;
                        bounced = true;
                    } else if (inverted) {
                        player.SuperBounce(Bottom);
                        bounced = true;
                    }
                }
            } else {
                throw new Exception("Orientation not supported!");
            }

            if (bounced) {
                // animate spring
                bounceAnimate.Invoke(this, noParams);

                // Restore original dash count.
                player.Dashes = originalDashCount;

                if (!refillStamina) {
                    // Also restore original stamina count.
                    player.Stamina = originalStamina;
                }

                RefillDashes(player);
            }
        }

        private void InvertedSuperBounce(Player player, float fromY) {
            player.SuperBounce(fromY);
            player.Speed.Y *= -1f;
            new DynData<Player>(player)["varJumpSpeed"] = player.Speed.Y;
        }

        protected virtual void RefillDashes(Player player) {
            // this is here for handy reuse by Custom Dash Refill Spring.
        }
    }
}
