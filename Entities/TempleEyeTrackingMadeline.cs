using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    public class TempleEyeTrackingMadeline {
        public static void Load() {
            On.Celeste.TempleEye.ctor += modTempleEyeConstructor;
            IL.Celeste.TempleEye.Update += modTempleEyeUpdate;
        }

        public static void Unload() {
            On.Celeste.TempleEye.ctor -= modTempleEyeConstructor;
            IL.Celeste.TempleEye.Update -= modTempleEyeUpdate;
        }

        // ================ Temple eye handling ================

        private static void modTempleEyeConstructor(On.Celeste.TempleEye.orig_ctor orig, TempleEye self, EntityData data, Vector2 offset) {
            orig(self, data, offset);
            new DynData<TempleEye>(self)["followMadeline"] = data.Bool("followMadeline");
        }

        private static void modTempleEyeUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.Before,
                i => i.OpCode == OpCodes.Ldarg_0,
                i => i.OpCode == OpCodes.Call && ((MethodReference) i.Operand).Name.Contains("get_Scene"),
                i => i.OpCode == OpCodes.Callvirt && ((MethodReference) i.Operand).Name.Contains("get_Tracker"),
                i => i.OpCode == OpCodes.Callvirt,
                // this is stloc.0 on the XNA branch and stloc.1 on the FNA branch
                i => (i.OpCode == OpCodes.Stloc_0 || i.OpCode == OpCodes.Stloc_1))) {

                Logger.Log("MaxHelpingHand/TempleEyeTrackingMadeline", $"Patching TempleEye at CIL index {cursor.Index} to be able to mod target");

                // replace "this.Scene.Tracker.GetEntity<TheoCrystal>" with "ReturnTrackedActor(this)"
                cursor.Index++;
                cursor.RemoveRange(3);
                cursor.EmitDelegate<Func<TempleEye, Actor>>(returnTrackedActor);
            }
        }

        private static Actor returnTrackedActor(TempleEye self) {
            if (new DynData<TempleEye>(self).Get<bool>("followMadeline")) {
                return self.Scene.Tracker.GetEntity<Player>();
            }
            return self.Scene.Tracker.GetEntity<TheoCrystal>();
        }
    }
}
