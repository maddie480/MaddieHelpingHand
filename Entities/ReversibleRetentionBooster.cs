using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReversibleRetentionBooster")]
    public class ReversibleRetentionBooster : Booster {
        private static Type playerDashCoroutineType;
        private static ILHook playerDashCoroutineHook;

        private static new ParticleType P_Burst;
        private static new ParticleType P_Appear;

        public static void LoadContent() {
            P_Burst = new ParticleType(Booster.P_Burst) {
                Color = Calc.HexToColor("D772FA")
            };
            P_Appear = new ParticleType(Booster.P_Appear) {
                Color = Calc.HexToColor("D772FA")
            };
        }

        public static void Load() {
            IL.Celeste.Booster.ctor_Vector2_bool += reskinBooster;
            On.Celeste.Booster.Update += reskinParticles;

            MethodInfo realDashCoroutine = typeof(Player).GetMethod("DashCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
            playerDashCoroutineType = realDashCoroutine.DeclaringType;
            playerDashCoroutineHook = new ILHook(realDashCoroutine, retainSpeedInBothDirections);
        }

        public static void Unload() {
            IL.Celeste.Booster.ctor_Vector2_bool -= reskinBooster;
            On.Celeste.Booster.Update -= reskinParticles;

            playerDashCoroutineHook?.Dispose();
            playerDashCoroutineType = null;
            playerDashCoroutineHook = null;
        }

        private static void reskinBooster(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("booster"))) {
                Logger.Log("MaxHelpingHand/ReversibleRetentionBooster", $"Modding booster skin at {cursor.Index} in IL for Booster constructor");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<string, Booster, string>>((orig, self) => {
                    if (self is ReversibleRetentionBooster) {
                        return "MaxHelpingHand_reversibleRetentionBooster";
                    }
                    return orig;
                });
            }
        }

        private static void reskinParticles(On.Celeste.Booster.orig_Update orig, Booster self) {
            if (self is not ReversibleRetentionBooster) {
                orig(self);
                return;
            }

            ParticleType vanillaAppear = Booster.P_Appear;
            Booster.P_Appear = P_Appear;

            orig(self);

            Booster.P_Appear = vanillaAppear;
        }

        private static void retainSpeedInBothDirections(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(instr => instr.MatchLdflda<Player>("beforeDashSpeed"))) {
                Logger.Log("MaxHelpingHand/ReversibleRetentionBooster", $"Making retention reversible at {cursor.Index} in IL for Player.DashCoroutine");

                // load "this" in advance for the stfld at the very end
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, playerDashCoroutineType.GetField("<>4__this"));

                // get this, but the actual Player this, not the this from the Player.<DashCoroutine>d__427 state machine :sparkles:
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, playerDashCoroutineType.GetField("<>4__this"));

                // we'll also need beforeDashSpeed and speed
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, playerDashCoroutineType.GetField("<>4__this"));
                cursor.Emit(OpCodes.Ldfld, typeof(Player).GetField("beforeDashSpeed", BindingFlags.NonPublic | BindingFlags.Instance));

                cursor.Emit(OpCodes.Ldloc_3);

                cursor.EmitDelegate<Func<Player, Vector2, Vector2, Vector2>>((self, beforeDashSpeed, speed) => {
                    if (self.CurrentBooster is ReversibleRetentionBooster) {
                        if (Math.Sign(beforeDashSpeed.X) == -Math.Sign(speed.X) && Math.Abs(beforeDashSpeed.X) > Math.Abs(speed.X)) {
                            // reverse beforeDashSpeed so that the retention direction is reversed!
                            return -beforeDashSpeed;
                        }
                    }
                    return beforeDashSpeed;
                });

                // the return value should be assigned to beforeDashSpeed
                cursor.Emit(OpCodes.Stfld, typeof(Player).GetField("beforeDashSpeed", BindingFlags.NonPublic | BindingFlags.Instance));
            }
        }

        public ReversibleRetentionBooster(EntityData data, Vector2 offset) : base(data, offset) {
            new DynData<Booster>(this)["particleType"] = P_Burst;
        }
    }
}
