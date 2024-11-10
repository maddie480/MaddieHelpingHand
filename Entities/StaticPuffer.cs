using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/StaticPuffer")]
    public class StaticPuffer : Puffer {
        public static void Load() {
            IL.Celeste.Puffer.ctor_Vector2_bool += onPufferConstructor;
            On.Celeste.Puffer.Explode += onPufferExplode;
            IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool += onExplodeLaunch;
        }

        public static void Unload() {
            IL.Celeste.Puffer.ctor_Vector2_bool -= onPufferConstructor;
            On.Celeste.Puffer.Explode -= onPufferExplode;
            IL.Celeste.Player.ExplodeLaunch_Vector2_bool_bool -= onExplodeLaunch;
        }

        private static int? currentDownboostTolerance;
        private static bool currentPufferFacesRight;

        private static void onPufferConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<SineWave>("Randomize"))) {
                Logger.Log("MaxHelpingHand/StaticPuffer", $"Injecting call to unrandomize puffer sine wave at {cursor.Index} in IL for Puffer constructor");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(Puffer).GetField("idleSine", BindingFlags.NonPublic | BindingFlags.Instance));
                cursor.EmitDelegate<Action<Puffer, SineWave>>((self, idleSine) => {
                    if (self is StaticPuffer) {
                        // unrandomize the initial pufferfish position.
                        idleSine.Reset();
                    }
                });
            }
        }

        private int downboostTolerance;

        public StaticPuffer(EntityData data, Vector2 offset) : base(data, offset) {
            // remove the sine wave component so that it isn't updated.
            Get<SineWave>()?.RemoveSelf();

            // give the puffer a different depth compared to the player to eliminate frame-precise inconsistencies.
            Depth = -1;

            downboostTolerance = data.Int("downboostTolerance", -1);
        }

        private static void onPufferExplode(On.Celeste.Puffer.orig_Explode orig, Puffer self) {
            if (self is StaticPuffer puffer) {
                currentDownboostTolerance = puffer.downboostTolerance;
                currentPufferFacesRight = new DynData<Puffer>(puffer).Get<Vector2>("facing").X > 0;
            }

            orig(self);

            currentDownboostTolerance = null;
        }

        private static void onExplodeLaunch(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(instr => instr.MatchStfld<Player>("Speed"))) {
                Logger.Log("MaxHelpingHand/StaticPuffer", $"Modifying explode launch speed at {cursor.Index} in IL for Player.ExplodeLaunch");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<Vector2, Player, Vector2, Vector2>>((orig, self, from) => {
                    if (currentDownboostTolerance == null) {
                        // the current explode launching does not come from a static puffer, don't mess with it!
                        return orig;
                    }

                    float offsetX = Math.Abs(self.Center.X - from.X);
                    if (offsetX <= currentDownboostTolerance) {
                        // we shall downboost
                        if (orig.Y != 0) {
                            // we're already downboosting
                            return orig;
                        }
                        return (self.Center - from).SafeNormalize(-Vector2.UnitY) * orig.Length();
                    } else {
                        // we shall not downboost
                        if (orig.Y == 0) {
                            // we're already not downboosting
                            return orig;
                        }
                        return Vector2.UnitX * orig.Length() * (currentPufferFacesRight ? 1 : -1);
                    }
                });
            }
        }
    }
}
