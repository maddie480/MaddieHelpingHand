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
            hookModPlayerOrigUpdate = new ILHook(typeof(Player).GetMethod("orig_Update", BindingFlags.NonPublic | BindingFlags.Instance)!, modPlayerOrigUpdate);
        }

        public static void Unload() {
            hookModPlayerOrigUpdate?.Dispose();
            hookModPlayerOrigUpdate = null;
        }

        private static void modPlayerOrigUpdate(ILContext il) {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNextBestFit(MoveType.Before,
                instr => instr.MatchLdarg0(),
                instr => instr.MatchLdfld<Player>("StateMachine"),
                instr => instr.MatchCallvirt<StateMachine>("get_State"),
                instr => instr.MatchLdcI4(18),
                instr => instr.MatchBeq(out _))) {
                ILLabel skipStateCheck = cursor.DefineLabel();
                
                cursor.EmitLdarg0();
                cursor.EmitDelegate(modStateCheck);
                cursor.EmitBrtrue(skipStateCheck);
                
                cursor.GotoNext(MoveType.After, instr => instr.MatchBeq(out _));
                cursor.MarkLabel(skipStateCheck);
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

        private static bool modStateCheck(Player player) {
            return player.Scene.Tracker.GetComponent<TriggerDuringReflectionFall>() != null;
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