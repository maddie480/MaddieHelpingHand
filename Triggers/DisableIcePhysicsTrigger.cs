using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [CustomEntity("MaxHelpingHand/DisableIcePhysicsTrigger")]
    class DisableIcePhysicsTrigger : Trigger {
        public static void Load() {
            IL.Celeste.Player.NormalUpdate += modPlayerNormalUpdate;
        }

        public static void Unload() {
            IL.Celeste.Player.NormalUpdate -= modPlayerNormalUpdate;
        }

        private static void modPlayerNormalUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Level>("get_CoreMode"))) {
                Logger.Log("MaxHelpingHand/DisableIcePhysicsTrigger", $"Patching ice mode checking at {cursor.Index} in IL for Player.NormalUpdate");

                cursor.EmitDelegate<Func<Session.CoreModes, Session.CoreModes>>(coreMode => {
                    if (MaxHelpingHandModule.Instance.Session.IcePhysicsDisabled) {
                        // pretend there is no core mode, so that the ground is not slippery anymore.
                        return Session.CoreModes.None;
                    }
                    return coreMode;
                });
            }
        }

        private bool disableIcePhysics;

        public DisableIcePhysicsTrigger(EntityData data, Vector2 offset) : base(data, offset) {
            disableIcePhysics = data.Bool("disableIcePhysics", true);
        }

        public override void OnEnter(Player player) {
            base.OnEnter(player);

            MaxHelpingHandModule.Instance.Session.IcePhysicsDisabled = disableIcePhysics;
        }
    }
}
