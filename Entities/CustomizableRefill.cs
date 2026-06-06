using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomizableRefill")]
    public class CustomizableRefill : Refill {
        internal static void Load() {
            On.Celeste.Refill.UpdateY += modUpdateY;
        }

        internal static void Unload() {
            On.Celeste.Refill.UpdateY -= modUpdateY;
        }

        private static void modUpdateY(On.Celeste.Refill.orig_UpdateY orig, Refill self) {
            if (self is CustomizableRefill refill)
                refill.RotatedUpdateY();
            else
                orig(self);
        }

        private float _rotationDegrees;
        public float rotationDegrees {
            get => _rotationDegrees;
            set {
                _rotationDegrees = value;
                outline.Rotation = sprite.Rotation = flash.Rotation = _rotationDegrees * Calc.DegToRad;
            }
        }

        public CustomizableRefill(EntityData data, Vector2 offset) : base(data, offset) {
            float respawnTime = data.Float("respawnTime", 2.5f);

            // wrap the original OnPlayer method to modify the respawnTimer if it gets reset to 2.5f.
            PlayerCollider collider = Get<PlayerCollider>();
            Action<Player> orig = collider.OnCollide;
            collider.OnCollide = player => {
                orig(player);
                if (respawnTimer == 2.5f) {
                    respawnTimer = respawnTime;
                }
            };

            if (!string.IsNullOrEmpty(data.Attr("sprite"))) {
                // replace the sprite.
                string spritePath = "objects/MaxHelpingHand/refill/" + data.Attr("sprite");
                outline.Texture = GFX.Game[spritePath + "/outline"];

                sprite.Path = spritePath + "/idle";
                sprite.Stop();
                sprite.ClearAnimations();
                sprite.AddLoop("idle", "", 0.1f);
                sprite.Play("idle");

                flash.Path = spritePath + "/flash";
                flash.ClearAnimations();
                flash.Add("flash", "", 0.05f);
            }

            if (!string.IsNullOrEmpty(data.Attr("shatterParticleColor1")) && !string.IsNullOrEmpty(data.Attr("shatterParticleColor2"))) {
                p_shatter = new ParticleType(P_Shatter) {
                    Color = Calc.HexToColor(data.Attr("shatterParticleColor1")),
                    Color2 = Calc.HexToColor(data.Attr("shatterParticleColor2"))
                };
            }

            if (!string.IsNullOrEmpty(data.Attr("glowParticleColor1")) && !string.IsNullOrEmpty(data.Attr("glowParticleColor2"))) {
                p_glow = new ParticleType(P_Glow) {
                    Color = Calc.HexToColor(data.Attr("glowParticleColor1")),
                    Color2 = Calc.HexToColor(data.Attr("glowParticleColor2"))
                };
                p_regen = new ParticleType(P_Regen) {
                    Color = Calc.HexToColor(data.Attr("glowParticleColor1")),
                    Color2 = Calc.HexToColor(data.Attr("glowParticleColor2"))
                };
            }

            // we can set the property right here, as the outline and sprites are non-null after the constructor
            rotationDegrees = data.Float("rotation");

            if (!data.Bool("wave", true)) {
                // freeze the sine wave and remove it so that it stops updating.
                SineWave sine = Get<SineWave>();
                Remove(sine);
                sine.Counter = 0f;
                RotatedUpdateY();
            }
        }

        private void RotatedUpdateY() {
            sprite.Position = flash.Position = bloom.Position = GetRotationVector(sine.Value * 2);
        }

        public Vector2 GetRotationVector(float length)
            => Calc.AngleToVector(Calc.Up + rotationDegrees * Calc.DegToRad, length);
    }
}
