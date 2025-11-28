using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/TempleEyeTrackingMadeline")]
    public class TempleEyeTrackingMadeline : TempleEye {
        public static void Load() {
            IL.Celeste.TempleEye.Added += modTempleEyeAdded;
            IL.Celeste.TempleEye.Update += modTempleEyeUpdate;
        }

        public static void Unload() {
            IL.Celeste.TempleEye.Added -= modTempleEyeAdded;
            IL.Celeste.TempleEye.Update -= modTempleEyeUpdate;
        }

        private string spriteDirectory;

        public TempleEyeTrackingMadeline(EntityData data, Vector2 offset) : base(data, offset) {
            spriteDirectory = data.Attr("spriteDirectory", defaultValue: "scenery/temple/eye");
        }

        private static void modTempleEyeAdded(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldstr && ((string) instr.Operand).StartsWith("scenery/temple/eye/"))) {
                Logger.Log("MaxHelpingHand/TempleEyeTrackingMadeline", $"Replacing sprite path {cursor.Prev.Operand} at {cursor.Index} in TempleEye.Added");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<string, TempleEye, string>>(replaceTempleEyeTexture);
            }
        }

        private static string replaceTempleEyeTexture(string orig, TempleEye self) {
            if (self is TempleEyeTrackingMadeline eye) {
                return orig.Replace("scenery/temple/eye/", eye.spriteDirectory + "/");
            }
            return orig;
        }

        private static void modTempleEyeUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNextBestFit(MoveType.After,
                i => i.OpCode == OpCodes.Ldarg_0,
                i => i.OpCode == OpCodes.Call && ((MethodReference) i.Operand).Name.Contains("get_Scene"),
                i => i.OpCode == OpCodes.Callvirt && ((MethodReference) i.Operand).Name.Contains("get_Tracker"),
                i => i.OpCode == OpCodes.Callvirt,
                // this is stloc.0 on the XNA branch and stloc.1 on the FNA branch
                i => (i.OpCode == OpCodes.Stloc_0 || i.OpCode == OpCodes.Stloc_1))) {

                Logger.Log("MaxHelpingHand/TempleEyeTrackingMadeline", $"Patching TempleEye at CIL index {cursor.Index} to be able to mod target");

                cursor.Index--;
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Entity, TempleEye, Entity>>(modTrackedActor);
            }
        }

        private static Entity modTrackedActor(Entity orig, TempleEye self) {
            if (self is TempleEyeTrackingMadeline) {
                return self.Scene.Tracker.GetEntity<Player>();
            }
            return orig;
        }
    }
}
