using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [Tracked]
    [CustomEntity("MaxHelpingHand/SpikeRefillController")]
    public class SpikeRefillController : Entity {
        private static readonly MethodInfo m_OrigUpdate = typeof(Player).GetMethod("orig_Update");
        private static ILHook Hook_OrigUpdate;

        public string Flag { get; private set; }
        public bool FlagInverted { get; private set; }

        private bool IsFlagSatisfied => string.IsNullOrWhiteSpace(Flag) || (FlagInverted ^ SceneAs<Level>().Session.GetFlag(Flag));

        public SpikeRefillController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            Flag = data.Attr("flag");
            FlagInverted = data.Bool("flagInverted");
        }

        public static void Load() {
            Hook_OrigUpdate = new ILHook(m_OrigUpdate, HookSpikeRefillPrevention);
        }

        public static void Unload() {
            Hook_OrigUpdate?.Dispose();
        }

        private static void HookSpikeRefillPrevention(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall("Monocle.Entity", "System.Boolean CollideCheck<Celeste.Spikes>(Microsoft.Xna.Framework.Vector2)"))) {
                Logger.Log(LogLevel.Error, "MaxHelpingHand/SpikeRefillController",
                    $"Could not find CollideCheck<Spikes> in {il.Method.FullName}!");
                return;
            }

            Logger.Log(LogLevel.Verbose, "MaxHelpingHand/SpikeRefillController",
                $"Hooking CollideCheck<Spikes> in {il.Method.FullName} @ {InstructionToString(cursor.Next)}");

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<bool, Player, bool>>(OverrideRefillPreventionInSpikes);
        }

        private static bool OverrideRefillPreventionInSpikes(bool collidedWithSpikes, Player self) {
            // return true to prevent refills
            if (!collidedWithSpikes)
                return collidedWithSpikes;

            return !(self.Scene.Tracker.GetEntity<SpikeRefillController>()?.IsFlagSatisfied ?? false);
        }

        // stringifying branch instructions crashes because of monomod, very cool
        private static string InstructionToString(Instruction instr) {
            Instruction toStringify = instr;
            if (instr.Operand is ILLabel target) {
                toStringify = Instruction.Create(toStringify.OpCode, target.Target);
                toStringify.Offset = instr.Offset;
            }
            return toStringify.ToString();
        }
    }
}
