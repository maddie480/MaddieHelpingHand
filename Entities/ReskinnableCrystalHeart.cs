using Celeste.Mod.Entities;
using Celeste.Mod.Meta;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReskinnableCrystalHeart")]
    [TrackedAs(typeof(HeartGem))]
    public class ReskinnableCrystalHeart : HeartGem {
        public static void Load() {
            IL.Celeste.HeartGem.Awake += onHeartGemAwake;
            On.Celeste.HeartGem.Collect += onHeartGemCollect;
        }

        public static void Unload() {
            IL.Celeste.HeartGem.Awake -= onHeartGemAwake;
            On.Celeste.HeartGem.Collect -= onHeartGemCollect;
        }

        private readonly string sprite;
        private readonly string ghostSprite;
        private readonly string particleColor;
        private readonly string flagOnCollect;
        private readonly bool flagInverted;
        private readonly bool disableGhostSprite;

        public ReskinnableCrystalHeart(EntityData data, Vector2 offset) : base(data, offset) {
            sprite = data.Attr("sprite");
            ghostSprite = data.Attr("ghostSprite");
            particleColor = data.Attr("particleColor");
            flagOnCollect = data.Attr("flagOnCollect");
            flagInverted = data.Bool("flagInverted");
            disableGhostSprite = data.Bool("disableGhostSprite");
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // despawn if already collected in session and if heart does not end level
            // (like vanilla hearts, but this condition is checked in Level.LoadLevel() so we do not get that behavior just by extending HeartGem)
            MapMetaModeProperties meta = SceneAs<Level>().Session.MapData.GetMeta();
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

                new DynData<HeartGem>(this)["shineParticle"] = particle;
            }
        }

        private static void onHeartGemAwake(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // mod the Disable Ghost Sprite option in
            if (cursor.TryGotoNext(MoveType.AfterLabel, instr => instr.MatchStfld<HeartGem>("IsGhost"))) {
                Logger.Log("MaxHelpingHand/ReskinnableCrystalHeart", $"Disabling ghost sprite at {cursor.Index} in HeartGem::Awake");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<bool, HeartGem, bool>>((orig, self) => {
                    if (self is ReskinnableCrystalHeart heart && heart.disableGhostSprite) {
                        return false;
                    }
                    return orig;
                });
            }

            // mod the Sprite and Ghost Sprite options in
            if (cursor.TryGotoNext(instr => instr.MatchCallvirt<SpriteBank>("Create"))) {
                Logger.Log("MaxHelpingHand/ReskinnableCrystalHeart", $"Reskinning crystal heart at {cursor.Index} in HeartGem::Awake");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<string, HeartGem, string>>((orig, self) => {
                    if (self is ReskinnableCrystalHeart heart) {
                        if (!heart.IsGhost && !string.IsNullOrEmpty(heart.sprite)) {
                            return heart.sprite;
                        }
                        if (heart.IsGhost && !string.IsNullOrEmpty(heart.ghostSprite)) {
                            return heart.ghostSprite;
                        }
                    }
                    return orig;
                });
            }
        }

        private static void onHeartGemCollect(On.Celeste.HeartGem.orig_Collect orig, HeartGem self, Player player) {
            if (self is ReskinnableCrystalHeart heart && !string.IsNullOrEmpty(heart.flagOnCollect)) {
                self.SceneAs<Level>().Session.SetFlag(heart.flagOnCollect, !heart.flagInverted);
            }

            orig(self, player);
        }
    }
}
