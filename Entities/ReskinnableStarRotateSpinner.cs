using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReskinnableStarRotateSpinner")]
    public class ReskinnableStarRotateSpinner : RotateSpinner {
        private ParticleType[] trailParticles;
        private Sprite sprite;
        private int colorID;
        private bool immuneToGuneline;

        private static ILHook modGunelineCollisionCheck = null;

        public static void LoadMods() {
            if (modGunelineCollisionCheck == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata() { Name = "Guneline", Version = new Version(1, 0, 0) })) {
                Type bulletType = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "Guneline.Guneline")?.GetType().Assembly
                    .GetType("Guneline.Bullet");

                modGunelineCollisionCheck = new ILHook(bulletType.GetMethod("CollisionCheck", BindingFlags.NonPublic | BindingFlags.Instance), turnOffGunelineCollisionCheck);
            }
        }

        public static void Unload() {
            modGunelineCollisionCheck?.Dispose();
            modGunelineCollisionCheck = null;
        }


        public ReskinnableStarRotateSpinner(EntityData data, Vector2 offset) : base(data, offset) {
            immuneToGuneline = data.Bool("immuneToGuneline");

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

        private static void turnOffGunelineCollisionCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchIsinst<RotateSpinner>())) {
                Logger.Log("MaxHelpingHand/ReskinnableStarRotateSpinner", $"Making spinner immune to Guneline at {cursor.Index} in IL for CollisionCheck");
                cursor.EmitDelegate<Func<RotateSpinner, RotateSpinner>>(spinner => {
                    if (spinner is ReskinnableStarRotateSpinner reskinnableSpinner && reskinnableSpinner.immuneToGuneline) {
                        return null;
                    }
                    return spinner;
                });
            }
        }
    }
}
