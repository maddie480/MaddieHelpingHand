using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomizableRefill")]
    class CustomizableRefill : Refill {
        public CustomizableRefill(EntityData data, Vector2 offset) : base(data, offset) {
            DynData<Refill> self = new DynData<Refill>(this);
            float respawnTime = data.Float("respawnTime", 2.5f);

            // wrap the original OnPlayer method to modify the respawnTimer if it gets reset to 2.5f.
            PlayerCollider collider = Get<PlayerCollider>();
            Action<Player> orig = collider.OnCollide;
            collider.OnCollide = player => {
                orig(player);
                if (self.Get<float>("respawnTimer") == 2.5f) {
                    self["respawnTimer"] = respawnTime;
                }
            };

            if (!string.IsNullOrEmpty(data.Attr("sprite"))) {
                // replace the sprite.
                string spritePath = "objects/MaxHelpingHand/refill/" + data.Attr("sprite");
                self.Get<Image>("outline").Texture = GFX.Game[spritePath + "/outline"];

                Sprite sprite = self.Get<Sprite>("sprite");
                sprite.Path = spritePath + "/idle";
                sprite.Stop();
                sprite.ClearAnimations();
                sprite.AddLoop("idle", "", 0.1f);
                sprite.Play("idle");

                Sprite flash = self.Get<Sprite>("flash");
                flash.Path = spritePath + "/flash";
                flash.ClearAnimations();
                flash.Add("flash", "", 0.05f);
            }

            if (!string.IsNullOrEmpty(data.Attr("shatterParticleColor1")) && !string.IsNullOrEmpty(data.Attr("shatterParticleColor2"))) {
                self["p_shatter"] = new ParticleType(P_Shatter) {
                    Color = Calc.HexToColor(data.Attr("shatterParticleColor1")),
                    Color2 = Calc.HexToColor(data.Attr("shatterParticleColor2"))
                };
            }

            if (!string.IsNullOrEmpty(data.Attr("glowParticleColor1")) && !string.IsNullOrEmpty(data.Attr("glowParticleColor2"))) {
                self["p_glow"] = new ParticleType(P_Glow) {
                    Color = Calc.HexToColor(data.Attr("glowParticleColor1")),
                    Color2 = Calc.HexToColor(data.Attr("glowParticleColor2"))
                };
                self["p_regen"] = new ParticleType(P_Regen) {
                    Color = Calc.HexToColor(data.Attr("glowParticleColor1")),
                    Color2 = Calc.HexToColor(data.Attr("glowParticleColor2"))
                };
            }

            if (!data.Bool("wave", true)) {
                // freeze the sine wave and remove it so that it stops updating.
                SineWave sine = Get<SineWave>();
                Remove(sine);
                sine.Counter = 0f;
            }
        }
    }
}
