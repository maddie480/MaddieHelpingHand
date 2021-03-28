using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReskinnableBillboard")]
    [TrackedAs(typeof(PlaybackBillboard))]
    class ReskinnableBillboard : PlaybackBillboard {
        private string borderTexture;

        public static void Load() {
            IL.Celeste.PlaybackBillboard.Awake += modAwake;
        }

        public static void Unload() {
            IL.Celeste.PlaybackBillboard.Awake -= modAwake;
        }

        public ReskinnableBillboard(EntityData data, Vector2 offset) : base(data, offset) {
            borderTexture = data.Attr("borderTexture", "scenery/tvSlices");

            if (!data.Bool("renderBloom", defaultValue: true)) {
                Remove(Get<CustomBloom>());
            }
        }

        private static void modAwake(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("scenery/tvSlices"))) {
                Logger.Log("MaxHelpingHand/ReskinnableBillboard", $"Making billboard reskinnable at {cursor.Index} in IL for PlaybackBillboard.Awake");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<string, PlaybackBillboard, string>>((orig, self) => {
                    if (self is ReskinnableBillboard b) {
                        return b.borderTexture;
                    }
                    return orig;
                });
            }
        }
    }
}
