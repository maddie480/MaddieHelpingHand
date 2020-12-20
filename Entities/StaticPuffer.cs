using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using Mono.Cecil.Cil;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/StaticPuffer")]
    class StaticPuffer : Puffer {
        public static void Load() {
            IL.Celeste.Puffer.ctor_Vector2_bool += onPufferConstructor;
        }

        public static void Unload() {
            IL.Celeste.Puffer.ctor_Vector2_bool -= onPufferConstructor;
        }

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

        public StaticPuffer(EntityData data, Vector2 offset) : base(data, offset) {
            // remove the sine wave component so that it isn't updated.
            Get<SineWave>()?.RemoveSelf();

            // give the puffer a different depth compared to the player to eliminate frame-precise inconsistencies.
            Depth = -1;
        }
    }
}
