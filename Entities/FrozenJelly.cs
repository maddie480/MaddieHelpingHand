using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/FrozenJelly")]
    public class FrozenJelly : Glider {
        public static void Load() {
            IL.Celeste.Glider.Update += cancelGravity;
            IL.Celeste.Player.NormalUpdate += slowDownVerticalMovement;
        }

        public static void Unload() {
            IL.Celeste.Glider.Update -= cancelGravity;
            IL.Celeste.Player.NormalUpdate -= slowDownVerticalMovement;
        }

        public FrozenJelly(EntityData data, Vector2 offset) : base(data, offset) {
            RecreateJellySpritesByHand(this, data.Attr("spriteDirectory", "MaxHelpingHand/jellies/frozenjelly"));
            if (data.Bool("glow")) {
                Add(new VertexLight(new Vector2(0f, -10f), Color.White, 1f, 16, 48));
                Add(new BloomPoint(new Vector2(0f, -10f), 0.5f, 16f));
            }
        }

        internal static void RecreateJellySpritesByHand(Glider jelly, string path) {
            Sprite sprite = new Sprite(GFX.Game, path + "/");
            sprite.AddLoop("idle", "idle", 0.1f);
            sprite.AddLoop("held", "held", 0.1f);
            sprite.Add("fall", "fall", 0.06f, "fallLoop");
            sprite.AddLoop("fallLoop", "fallLoop", 0.06f);
            sprite.Add("death", "death", 0.06f);
            sprite.JustifyOrigin(new Vector2(0.5f, 0.58f));
            sprite.Play("idle");

            DynData<Glider> jellyData = new DynData<Glider>(jelly);
            jelly.Remove(jellyData.Get<Sprite>("sprite"));
            jellyData["sprite"] = sprite;
            jelly.Add(sprite);
        }

        private static void cancelGravity(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(30f))) {
                Logger.Log("MaxHelpingHand/FrozenJelly", $"Changing terminal falling velocity of jelly at {cursor.Index} in IL for Glider.Update");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Glider, float>>(modGravity);
            }
        }

        private static float modGravity(float orig, Glider self) {
            return (self is FrozenJelly) ? 0f : orig;
        }

        private static void slowDownVerticalMovement(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(120f) || instr.MatchLdcR4(-32f) || instr.MatchLdcR4(24f) || instr.MatchLdcR4(40f))) {
                Logger.Log("MaxHelpingHand/FrozenJelly", $"Changing terminal falling velocity of player at {cursor.Index} in IL for Player.NormalUpdate (constant = {cursor.Prev.Operand})");

                bool lastMatched = (float) cursor.Prev.Operand == 40f;
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Player, float>>(modVerticalMovement);

                if (lastMatched) {
                    break;
                }
            }
        }

        private static float modVerticalMovement(float orig, Player self) {
            return (self.Holding?.Entity is FrozenJelly) ? (orig / 2f) : orig;
        }
    }
}