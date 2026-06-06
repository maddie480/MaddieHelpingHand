using Celeste.Mod.Helpers;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Triggers {
    [Tracked]
    public class TriggerDuringReflectionFall() : Component(false, false) {
        private static ILHook hookModPlayerOrigUpdate;

        public static void Load() {
            hookModPlayerOrigUpdate = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.Public | BindingFlags.Instance)!, modPlayerOrigUpdate);
        }

        public static void Unload() {
            hookModPlayerOrigUpdate?.Dispose();
            hookModPlayerOrigUpdate = null;
        }

        private static void modPlayerOrigUpdate(ILContext il) {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNextBestFit(MoveType.Before,
                instr => instr.MatchLdcI4(Player.StReflectionFall),
                instr => instr.MatchBeq(out _))) {
                cursor.EmitLdarg0();
                cursor.EmitDelegate(modStateCheck);
            }

            ILLabel loopStart = null;
            if (cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchBr(out loopStart),
                instr => instr.MatchLdloca(9),
                instr => instr.MatchCall<List<Entity>.Enumerator>("get_Current"),
                instr => instr.MatchCastclass<Trigger>(),
                instr => instr.MatchStloc(10))) {
                cursor.EmitLdarg0();
                cursor.EmitLdloc(10);
                cursor.EmitDelegate(modSkipTrigger);
                cursor.EmitBrtrue(loopStart!);
            }
        }

        private static int modStateCheck(int origState, Player player) {
            return player.Scene.Tracker.GetComponent<TriggerDuringReflectionFall>() == null ? origState : -1;
        }

        private static bool modSkipTrigger(Player player, Trigger trigger) {
            return player.StateMachine.State == Player.StReflectionFall && trigger.Get<TriggerDuringReflectionFall>() == null;
        }

        public override void Added(Entity entity) {
            if (entity is not Trigger) {
                throw new InvalidOperationException("Cannot add a TriggerDuringReflectionFall component to something that is not a Trigger!");
            }

            base.Added(entity);
        }
    }
}