using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/ReverseJelly")]
    public class ReverseJelly : Glider {
        public static void Load() {
            IL.Celeste.Glider.Update += reverseYAxis;
            IL.Celeste.Player.NormalUpdate += reverseYAxisPlayer;
            IL.Celeste.Player.Throw += reverseYAxisThrow;
        }

        public static void Unload() {
            IL.Celeste.Glider.Update -= reverseYAxis;
            IL.Celeste.Player.NormalUpdate -= reverseYAxisPlayer;
            IL.Celeste.Player.Throw -= reverseYAxisThrow;
        }

        public ReverseJelly(EntityData data, Vector2 offset) : base(data, offset) {
            FrozenJelly.RecreateJellySpritesByHand(this, data.Attr("spriteDirectory", "MaxHelpingHand/jellies/reversejelly"));
            if (data.Bool("glow")) {
                Add(new VertexLight(new Vector2(0f, -10f), Color.White, 1f, 16, 48));
                Add(new BloomPoint(new Vector2(0f, -10f), 0.5f, 16f));
            }
        }

        private static void reverseYAxis(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdsfld(typeof(Input), "GliderMoveY"),
                instr => instr.MatchLdfld<VirtualIntegerAxis>("Value"))) {

                Logger.Log("MaxHelpingHand/ReverseJelly", $"Reversing jelly Y axis at {cursor.Index} in IL for Glider.Update");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, Glider, int>>((orig, self) => (self is ReverseJelly) ? (-orig) : orig);
            }
        }

        private static void reverseYAxisPlayer(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext((MoveType) 2,
                instr => instr.MatchLdsfld(typeof(Input), "GliderMoveY"),
                instr => instr.MatchCall<VirtualIntegerAxis>("op_Implicit"))) {

                object operand = cursor.Prev.Operand;
                bool returnsFloat = (operand as MethodReference).ReturnType.FullName == "System.Single";
                Logger.Log("MaxHelpingHand/ReverseJelly", $"Reversing jelly Y axis at {cursor.Index} in IL for Player.NormalUpdate");

                cursor.Emit(OpCodes.Ldarg_0);
                if (returnsFloat) {
                    cursor.EmitDelegate<Func<float, Player, float>>((orig, self) => (self.Holding?.Entity is ReverseJelly) ? -orig : orig);
                } else {
                    cursor.EmitDelegate<Func<int, Player, int>>((orig, self) => (self.Holding?.Entity is ReverseJelly) ? -orig : orig);
                }
            }
        }

        private static void reverseYAxisThrow(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdsfld(typeof(Input), "MoveY"),
                instr => instr.MatchLdfld<VirtualIntegerAxis>("Value"))) {

                Logger.Log("MaxHelpingHand/ReverseJelly", $"Reversing jelly Y axis at {cursor.Index} in IL for Player.Throw");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, Player, int>>((orig, self) => (self.Holding?.Entity is ReverseJelly) ? -orig : orig);
            }

            cursor.Index = 0;

            while (cursor.TryGotoNext(MoveType.After, instr => ILPatternMatchingExt.MatchLdfld<Player>(instr, "Facing"))) {
                Logger.Log("MaxHelpingHand/ReverseJelly", $"Reversing facing at {cursor.Index} in IL for Player.Throw");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Facings, Player, Facings>>((orig, self) => {
                    if (!(self.Holding?.Entity is ReverseJelly)) {
                        return orig;
                    }
                    return (orig != Facings.Right) ? Facings.Right : Facings.Left;
                });
            }
        }
    }
}