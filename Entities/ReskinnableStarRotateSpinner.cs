using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReskinnableStarRotateSpinner")]
    class ReskinnableStarRotateSpinner : RotateSpinner {
        private ParticleType[] trailParticles;
        private Sprite sprite;
        private int colorID;

        public ReskinnableStarRotateSpinner(EntityData data, Vector2 offset) : base(data, offset) {
            string[] particleColorsAsStrings = data.Attr("particleColors", "EA64B7|3EE852,67DFEA|E85351,EA582C|33BDE8").Split(',');
            trailParticles = new ParticleType[particleColorsAsStrings.Length];
            for (int i = 0; i < particleColorsAsStrings.Length; i++) {
                string[] colors = particleColorsAsStrings[i].Split('|');
                trailParticles[i] = new ParticleType(StarTrackSpinner.P_Trail[0]) {
                    Color = Calc.HexToColor(colors[0]),
                    Color2 = Calc.HexToColor(colors[1])
                };
            }

            colorID = Calc.Random.Next(0, particleColorsAsStrings.Length);

            Add(sprite = new Sprite(GFX.Game, data.Attr("spriteFolder", "danger/MaxHelpingHand/starSpinner") + "/"));
            for (int i = 0; i < particleColorsAsStrings.Length; i++) {
                sprite.AddLoop($"idle{i}", $"idle{i}_", 0.08f);
                sprite.Add($"spin{i}", $"spin{i}_", 0.06f, $"idle{(i + 1) % particleColorsAsStrings.Length}");
            }
            sprite.CenterOrigin();
            sprite.Play($"idle{colorID}");

            Depth = -50;
            Add(new MirrorReflection());
        }

        public override void Update() {
            base.Update();
            if (Moving && Scene.OnInterval(0.03f)) {
                SceneAs<Level>().ParticlesBG.Emit(trailParticles[(colorID + 1) % trailParticles.Length], 1, Position, Vector2.One * 3f);
            }
            if (Scene.OnInterval(0.8f)) {
                colorID++;
                colorID %= trailParticles.Length;
                sprite.Play("spin" + colorID);
            }
        }
    }
}
