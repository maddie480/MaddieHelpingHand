using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;
using System.Reflection;
using Celeste.Mod.Helpers;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReskinnableCrystalHeart")]
    [TrackedAs(typeof(HeartGem))]
    public class ReskinnableCrystalHeart : HeartGem {
        private static Hook altSidesHelperHook = null;

        public static void Load() {
            IL.Celeste.HeartGem.Awake += onHeartGemAwake;
            On.Celeste.HeartGem.Collect += onHeartGemCollect;
            IL.Celeste.HeartGem.Update += modHeartGemUpdate;
        }

        public static void Unload() {
            IL.Celeste.HeartGem.Awake -= onHeartGemAwake;
            On.Celeste.HeartGem.Collect -= onHeartGemCollect;
            IL.Celeste.HeartGem.Update -= modHeartGemUpdate;

            altSidesHelperHook?.Dispose();
            altSidesHelperHook = null;
        }

        public static void HookMods() {
            if (altSidesHelperHook == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "AltSidesHelper", Version = new Version(1, 6, 0) })) {
                Type altSidesHelperModule = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "AltSidesHelper.AltSidesHelperModule")?.GetType();
                altSidesHelperHook = new Hook(altSidesHelperModule.GetMethod("SetCrystalHeartSprite", BindingFlags.NonPublic | BindingFlags.Instance),
                    typeof(ReskinnableCrystalHeart).GetMethod("disableAltSidesHelperReskinning", BindingFlags.NonPublic | BindingFlags.Static));
            }
        }

        private readonly string overrideSprite;
        private readonly string ghostSprite;
        private readonly string particleColor;
        private readonly string flagOnCollect;
        private readonly bool flagInverted;
        private readonly bool disableGhostSprite;

        private float _rotationDegrees;
        public float rotationDegrees {
            get => _rotationDegrees;
            set {
                _rotationDegrees = value;
                if (sprite is not null)
                    sprite.Rotation = _rotationDegrees * Calc.DegToRad;
            }
        }

        public ReskinnableCrystalHeart(EntityData data, Vector2 offset) : base(data, offset) {
            overrideSprite = data.Attr("sprite");
            ghostSprite = data.Attr("ghostSprite");
            particleColor = data.Attr("particleColor");
            flagOnCollect = data.Attr("flagOnCollect");
            flagInverted = data.Bool("flagInverted");
            disableGhostSprite = data.Bool("disableGhostSprite");

            // need to set sprite rotation in Awake, as this is where it's set
            _rotationDegrees = data.Float("rotation");
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // despawn if already collected in session and if heart does not end level
            // (like vanilla hearts, but this condition is checked in Level.LoadLevel() so we do not get that behavior just by extending HeartGem)
            MapMetaModeProperties meta = SceneAs<Level>().Session.MapData.Meta;
            bool heartIsEnd = (meta != null && meta.HeartIsEnd.GetValueOrDefault());
            if (SceneAs<Level>().Session.HeartGem && !heartIsEnd) {
                RemoveSelf();
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // "particle color" option
            if (!string.IsNullOrEmpty(particleColor)) {
                ParticleType particle = new ParticleType(P_BlueShine) {
                    Color = Calc.HexToColor(particleColor)
                };

                shineParticle = particle;
            }

            sprite.Rotation = _rotationDegrees * Calc.DegToRad;
        }

        private void RotatedWiggle() {
            sprite.Position = GetRotationVector(MathF.Sin(timer * 2f) * 2f) + moveWiggleDir * (moveWiggler.Value * -8f);
        }

        public Vector2 GetRotationVector(float length)
            => Calc.AngleToVector(Calc.Up + rotationDegrees * Calc.DegToRad, length);

        private static void onHeartGemAwake(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // mod the Disable Ghost Sprite option in
            if (cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchStfld<HeartGem>("IsGhost"))) {
                Logger.Log("MaxHelpingHand/ReskinnableCrystalHeart", $"Disabling ghost sprite at {cursor.Index} in HeartGem::Awake");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<bool, HeartGem, bool>>(modGhostSprite);
            }

            // mod the Sprite and Ghost Sprite options in
            if (cursor.TryGotoNext(instr => instr.MatchCallvirt<SpriteBank>("Create"))) {
                Logger.Log("MaxHelpingHand/ReskinnableCrystalHeart", $"Reskinning crystal heart at {cursor.Index} in HeartGem::Awake");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<string, HeartGem, string>>(modHeartSprites);
            }
        }

        private static bool modGhostSprite(bool orig, HeartGem self) {
            if (self is ReskinnableCrystalHeart heart && heart.disableGhostSprite) {
                return false;
            }
            return orig;
        }

        private static string modHeartSprites(string orig, HeartGem self) {
            if (self is ReskinnableCrystalHeart heart) {
                if (!heart.IsGhost && !string.IsNullOrEmpty(heart.overrideSprite)) {
                    return heart.overrideSprite;
                }
                if (heart.IsGhost && !string.IsNullOrEmpty(heart.ghostSprite)) {
                    return heart.ghostSprite;
                }
            }
            return orig;
        }

        private static void onHeartGemCollect(On.Celeste.HeartGem.orig_Collect orig, HeartGem self, Player player) {
            if (self is ReskinnableCrystalHeart heart && !string.IsNullOrEmpty(heart.flagOnCollect)) {
                self.SceneAs<Level>().Session.SetFlag(heart.flagOnCollect, !heart.flagInverted);
            }

            orig(self, player);
        }

        private static void modHeartGemUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we need to mess with the y oscillation of the heart.
            // the il making up this section is veeeeeery long, so let's just match the beginning and the end.
            // ...and Pray
            if (!cursor.TryGotoNextBestFit(
                MoveType.Before,
                static instr => instr.MatchLdarg(0),
                static instr => instr.MatchLdfld<HeartGem>("sprite"),
                static instr => instr.MatchCall<Vector2>("get_UnitY")))
                return;

            ILLabel skipWiggle = cursor.DefineLabel();
            int emitIndex = cursor.Index;

            if (!cursor.TryGotoNextBestFit(
                MoveType.After,
                static instr => instr.MatchLdcR4(-8),
                static instr => instr.MatchCall<Vector2>("op_Multiply"),
                static instr => instr.MatchCall<Vector2>("op_Addition"),
                static instr => instr.MatchStfld<GraphicsComponent>("Position")))
                return;

            cursor.MarkLabel(skipWiggle);
            cursor.Index = emitIndex;

            Logger.Log("MaxHelpingHand/ReskinnableCrystalHeart", $"Inserting rotated Y oscillation and branch at {cursor.Index} in IL for HeartGem.Update");

            cursor.EmitLdarg0();
            cursor.EmitDelegate(tryRotatedWiggle);
            cursor.EmitBrtrue(skipWiggle);
        }

        private static bool tryRotatedWiggle(HeartGem self) {
            if (self is not ReskinnableCrystalHeart heart)
                return false;

            heart.RotatedWiggle();
            return true;
        }

        private static void disableAltSidesHelperReskinning(Action<EverestModule, On.Celeste.HeartGem.orig_Awake, HeartGem, Scene> orig, EverestModule self,
             On.Celeste.HeartGem.orig_Awake origAwake, HeartGem selfAwake, Scene scene) {

            // we're hooking a hook on HeartGem.Awake, so we get the orig/self from the hook, and the orig/self from the vanilla method.

            // only mess with reskinnable crystal hearts.
            if (!(selfAwake is ReskinnableCrystalHeart heart)) {
                orig(self, origAwake, selfAwake, scene);
                return;
            }


            AreaKey area = (heart.Scene as Level).Session.Area;
            bool isGhost = !heart.IsFake && SaveData.Instance.Areas_Safe[area.ID].Modes[(int) area.Mode].HeartGem;

            // only mess with actually reskinned crystal hearts.
            bool isReskinned;
            if (isGhost && !heart.disableGhostSprite) {
                isReskinned = !string.IsNullOrEmpty(heart.ghostSprite);
            } else {
                isReskinned = !string.IsNullOrEmpty(heart.overrideSprite);
            }

            if (isReskinned) {
                // skip the Alt Sides Helper method in order to make sure the crystal heart is reskinned.
                origAwake(selfAwake, scene);
            } else {
                // do not touch anything!
                orig(self, origAwake, selfAwake, scene);
            }
        }
    }
}
