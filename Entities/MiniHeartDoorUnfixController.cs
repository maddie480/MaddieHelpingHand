using Celeste;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // This controller reverts this fix, and was made to fix a map from Etpio in FLCC: https://github.com/EverestAPI/CelesteCollabUtils2/pull/60
    [CustomEntity("MaxHelpingHand/MiniHeartDoorUnfixController")]
    [Tracked]
    class MiniHeartDoorUnfixController : Entity {
        private static ILHook miniHeartDoorHook = null;

        public static void Initialize() {
            if (miniHeartDoorHook != null) return;

            Type miniHeartDoorClass = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "Celeste.Mod.CollabUtils2.CollabModule")?.GetType().Assembly
                .GetType("Celeste.Mod.CollabUtils2.Entities.MiniHeartDoor");

            if (miniHeartDoorClass != null) {
                miniHeartDoorHook = new ILHook(miniHeartDoorClass.GetMethod("Update"), modMiniHeartDoorUpdate);
            }
        }

        public static void Unload() {
            miniHeartDoorHook?.Dispose();
            miniHeartDoorHook = null;
        }

        private static void modMiniHeartDoorUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while(cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<HeartGemDoor>("get_Opened"))) {
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<bool, HeartGemDoor, bool>>((orig, self) => {
                    if (self.Scene.Tracker.CountEntities<MiniHeartDoorUnfixController>() > 0) {
                        // this condition wasn't here before the fix, so we want to make it true!
                        return true;
                    }

                    return orig;
                });
            }
        }

        public MiniHeartDoorUnfixController(EntityData data, Vector2 offset) : base(data.Position + offset) { }
    }
}
