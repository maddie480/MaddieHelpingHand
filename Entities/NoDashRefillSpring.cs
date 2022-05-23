using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/NoDashRefillSpring", "MaxHelpingHand/NoDashRefillSpringLeft", "MaxHelpingHand/NoDashRefillSpringRight")]
    [Tracked(inherited: true)]
    public class NoDashRefillSpring : Spring {
        public static void Load() {
            On.Celeste.LightingRenderer.BeforeRender += onLightingBeforeRender;
        }

        public static void Unload() {
            On.Celeste.LightingRenderer.BeforeRender -= onLightingBeforeRender;
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

        private static MethodInfo bounceAnimate = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.NonPublic | BindingFlags.Instance);
        private static object[] noParams = new object[0];

        private readonly bool ignoreLighting;
        private readonly bool refillStamina;

        public NoDashRefillSpring(EntityData data, Vector2 offset)
            : base(data.Position + offset, GetOrientationFromName(data.Name), data.Bool("playerCanUse", true)) {

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
                default:
                    throw new Exception("Custom Dash Refill Spring name doesn't correlate to a valid Orientation!");
            }
        }


        private void OnCollide(Player player) {
            if (player.StateMachine.State == 9) {
                return;
            }

            // Save dash and stamina count. Dashes / stamina are reloaded by SideBounce and SuperBounce.
            int originalDashCount = player.Dashes;
            float originalStamina = player.Stamina;

            if (Orientation == Orientations.Floor) {
                if (player.Speed.Y >= 0f) {
                    bounceAnimate.Invoke(this, noParams);
                    player.SuperBounce(Top);
                }
            } else if (Orientation == Orientations.WallLeft) {
                if (player.SideBounce(1, Right, CenterY)) {
                    bounceAnimate.Invoke(this, noParams);
                }
            } else if (Orientation == Orientations.WallRight) {
                if (player.SideBounce(-1, Left, CenterY)) {
                    bounceAnimate.Invoke(this, noParams);
                }
            } else {
                throw new Exception("Orientation not supported!");
            }

            // Restore original dash count.
            player.Dashes = originalDashCount;

            if (!refillStamina) {
                // Also restore original stamina count.
                player.Stamina = originalStamina;
            }

            RefillDashes(player);
        }

        protected virtual void RefillDashes(Player player) {
            // this is here for handy reuse by Custom Dash Refill Spring.
        }
    }
}
