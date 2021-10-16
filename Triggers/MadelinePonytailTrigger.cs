using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/MadelinePonytailTrigger")]
    public class MadelinePonytailTrigger : Trigger {
        private static readonly Color mainHairColor = Calc.HexToColor("CD6759");
        private static readonly Color hairTieColor = Calc.HexToColor("8C0F76");
        private static ParticleType hairParticle;

        public static void Load() {
            IL.Celeste.PlayerHair.Render += hookHairScaleAndHairCount;
            IL.Celeste.PlayerHair.AfterUpdate += hookHairCount;
            On.Celeste.PlayerHair.GetHairColor += hookHairColor;
            On.Celeste.Player.DashUpdate += hookParticleColor;
            IL.Celeste.PlayerHair.ctor += hookHairCount;
        }

        public static void Unload() {
            IL.Celeste.PlayerHair.Render -= hookHairScaleAndHairCount;
            IL.Celeste.PlayerHair.AfterUpdate -= hookHairCount;
            On.Celeste.PlayerHair.GetHairColor -= hookHairColor;
            On.Celeste.Player.DashUpdate -= hookParticleColor;
            IL.Celeste.PlayerHair.ctor -= hookHairCount;
        }

        public static void LoadContent() {
            hairParticle = new ParticleType(Player.P_DashA) {
                Color = mainHairColor,
                Color2 = Calc.HexToColor("AF584D")
            };
        }

        private static void hookHairScaleAndHairCount(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // === hook hair scale: we don't hook GetHairScale because of the curse around hooking methods returning Vector2.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<PlayerHair>("GetHairScale"))) {
                Logger.Log("MaxHelpingHand/MadelinePonytailTrigger", $"Modifying Madeline hair scale at {cursor.Index} in IL for PlayerHair.Render");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(cursor.Prev.Previous.Previous.OpCode, cursor.Prev.Previous.Previous.Operand); // load the index local variable.
                cursor.EmitDelegate<Func<Vector2, PlayerHair, int, Vector2>>((orig, self, index) => {
                    if (new Func<bool>(() => MaxHelpingHandModule.Instance.Session.MadelineHasPonytail && self.Entity is Player && index != 0)()) {
                        // shrink Maddy's hair, except for index 0 (over her head) to avoid... shrinking her head.
                        float scale = 0.25f + (1f - index / 6f) * 0.6f;
                        return new Vector2(scale * 0.75f * Math.Abs(self.Sprite.Scale.X), scale * 0.75f);
                    }
                    return orig;
                });
            }

            // reset the cursor
            cursor.Index = 0;

            // === hook "hair count" (Maddy's hair length).
            hookHairCount(cursor);
        }

        private static void hookHairCount(ILContext il) {
            hookHairCount(new ILCursor(il));
        }

        private static void hookHairCount(ILCursor cursor) {
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<PlayerSprite>("HairCount"))) {
                Logger.Log("MaxHelpingHand/MadelinePonytailTrigger", $"Modifying Madeline hair size at {cursor.Index} in IL for PlayerHair.Render");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, PlayerHair, int>>((orig, self) => {
                    if (MaxHelpingHandModule.Instance.Session.MadelineHasPonytail && (self.Entity == null || self.Entity is Player)) {
                        // make Madeline's hair longer.
                        return 6;
                    }
                    return orig;
                });
            }
        }

        private static Color hookHairColor(On.Celeste.PlayerHair.orig_GetHairColor orig, PlayerHair self, int index) {
            if (MaxHelpingHandModule.Instance.Session.MadelineHasPonytail && self.Entity is Player p) {
                if (index == 1) {
                    // "hair tie" near the top of the hair.
                    return hairTieColor * self.Alpha;
                }
                if (index != 5 || p.Dashes == 1) {
                    // we want the hair, except the tip, to have our custom hair color. We also want to recolor the 1-dash color entirely.
                    return mainHairColor * self.Alpha;
                }
            }
            return orig(self, index);
        }

        private static int hookParticleColor(On.Celeste.Player.orig_DashUpdate orig, Player self) {
            if (!MaxHelpingHandModule.Instance.Session.MadelineHasPonytail) {
                return orig(self);
            }

            // back up vanilla particles
            ParticleType bakDashB = Player.P_DashB;
            ParticleType bakDashBadB = Player.P_DashBadB;

            // replace them with our recolored ones
            Player.P_DashB = hairParticle;
            Player.P_DashBadB = hairParticle;

            // run vanilla code: if it emits particles, it will use our recolored ones.
            int result = orig(self);

            // restore vanilla particles
            Player.P_DashB = bakDashB;
            Player.P_DashBadB = bakDashBadB;

            return result;
        }


        // ===== Trigger code

        private readonly bool enable;

        public MadelinePonytailTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            enable = data.Bool("enable", defaultValue: true);
        }

        public override void OnEnter(Player player) {
            MaxHelpingHandModule.Instance.Session.MadelineHasPonytail = enable;
        }
    }
}
