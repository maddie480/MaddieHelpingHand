using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/CameraCatchupSpeedTrigger")]
    [Tracked]
    public class CameraCatchupSpeedTrigger : Trigger {
        private static ILHook playerOrigUpdateHook;

        public static void Load() {
            using (new DetourContext() { After = { "*" } }) { // be sure to be applied after Spring Collab 2020 because we don't want to break it
                playerOrigUpdateHook = new ILHook(typeof(Player).GetMethod("orig_Update"), modPlayerOrigUpdate);
            }
        }

        public static void Unload() {
            playerOrigUpdateHook?.Dispose();
        }

        private static void modPlayerOrigUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we're looking for: 1f - (float)Math.Pow(0.01f / num2, Engine.DeltaTime)
            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdcR4(0.01f),
                instr => instr.OpCode == OpCodes.Ldloc_S)) {

                Logger.Log("MaxHelpingHand/CameraCatchupSpeedTrigger", $"Inserting code to mod camera catchup speed at {cursor.Index} in IL for Player.orig_Update()");

                // this delegate will allow us to turn num2 into something else.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Player, float>>((orig, self) => {
                    CameraCatchupSpeedTrigger trigger = self.CollideFirst<CameraCatchupSpeedTrigger>();
                    if (trigger != null) {
                        return trigger.catchupSpeed;
                    }
                    return MaxHelpingHandModule.Instance.Session.CameraCatchupSpeed ?? orig;
                });
            }
        }


        private float catchupSpeed;
        private bool revertOnLeave;

        public CameraCatchupSpeedTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            catchupSpeed = data.Float("catchupSpeed", 1f);
            revertOnLeave = data.Bool("revertOnLeave", true);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            if (!revertOnLeave) {
                MaxHelpingHandModule.Instance.Session.CameraCatchupSpeed = catchupSpeed;
            }
        }
    }
}