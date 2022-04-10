using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/InstantLavaBlockerTrigger")]
    [Tracked]
    public class InstantLavaBlockerTrigger : LavaBlockerTrigger {
        private static ILHook frostHelperHook = null;

        public static void Load() {
            IL.Celeste.RisingLava.Update += onRisingLavaUpdate;
        }

        public static void HookMods() {
            if (frostHelperHook == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "FrostHelper", Version = new Version(1, 34, 4) })) {
                Type risingLavaClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "FrostHelper.FrostModule")?.GetType().Assembly
                    .GetType("FrostHelper.CustomRisingLava");

                frostHelperHook = new ILHook(risingLavaClass.GetMethod("Update"), onRisingLavaUpdate);
            }
        }

        public static void Unload() {
            IL.Celeste.RisingLava.Update -= onRisingLavaUpdate;

            frostHelperHook?.Dispose();
            frostHelperHook = null;
        }

        private readonly bool canReenter;

        public InstantLavaBlockerTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            canReenter = data.Bool("canReenter");
        }

        private static void onRisingLavaUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(instr => instr.MatchLdfld<Player>("JustRespawned"))) {
                Logger.Log("MaxHelpingHand/InstantLavaBlockerTrigger", $"Injecting code to block lava instantly at {cursor.Index} in IL for {il.Method.FullName}");

                cursor.Emit(OpCodes.Dup);
                cursor.Index++;
                cursor.EmitDelegate<Func<Player, bool, bool>>((player, orig) => {
                    if (orig && player.CollideCheck<InstantLavaBlockerTrigger>()) {
                        // pretend the player didn't just respawn to disable some code that makes the lava advance in that specific case.
                        return false;
                    }

                    return orig;
                });
            }
        }

        public override void OnLeave(Player player) {
            base.OnLeave(player);

            if (!canReenter) {
                RemoveSelf();
            }
        }
    }
}
