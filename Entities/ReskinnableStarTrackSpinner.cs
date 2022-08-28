using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReskinnableStarTrackSpinner")]
    public class ReskinnableStarTrackSpinner : TrackSpinner {
        private ParticleType[] trailParticles;
        private Sprite sprite;
        private bool hasStarted;
        private int colorID;
        private bool trail;
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

        public ReskinnableStarTrackSpinner(EntityData data, Vector2 offset) : base(data, offset) {
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
            if (trail && Scene.OnInterval(0.03f)) {
                SceneAs<Level>().ParticlesBG.Emit(trailParticles[colorID], 1, Position, Vector2.One * 3f);
            }
        }

        public override void OnTrackStart() {
            colorID++;
            colorID %= trailParticles.Length;
            sprite.Play("spin" + colorID);
            if (hasStarted) {
                Audio.Play("event:/game/05_mirror_temple/bladespinner_spin", Position);
            }
            hasStarted = true;
            trail = true;
        }

        public override void OnTrackEnd() {
            trail = false;
        }

        private static void turnOffGunelineCollisionCheck(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchIsinst<TrackSpinner>())) {
                Logger.Log("MaxHelpingHand/ReskinnableStarTrackSpinner", $"Making spinner immune to Guneline at {cursor.Index} in IL for CollisionCheck");
                cursor.EmitDelegate<Func<TrackSpinner, TrackSpinner>>(spinner => {
                    if (spinner is ReskinnableStarTrackSpinner reskinnableSpinner && reskinnableSpinner.immuneToGuneline) {
                        return null;
                    }
                    return spinner;
                });
            }
        }
    }
}
