using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // For those that want to reskin their switch gates without all the flag stuff.
    [CustomEntity("MaxHelpingHand/ReskinnableSwitchGate")]
    [TrackedAs(typeof(SwitchGate))]
    public class ReskinnableSwitchGate : SwitchGate {
        private static string iconPath = null;

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            // we replace the icon using an IL hook, because a fair bit of setup happens in the constructor, and replacing the icon after that is less convenient.
            iconPath = entityData.Attr("icon");
            Entity result = new ReskinnableSwitchGate(entityData, offset);
            iconPath = null;
            return result;
        }

        public static void Load() {
            IL.Celeste.SwitchGate.ctor_Vector2_float_float_Vector2_bool_string += modSwitchGateConstructor;
        }

        public static void Unload() {
            IL.Celeste.SwitchGate.ctor_Vector2_float_float_Vector2_bool_string -= modSwitchGateConstructor;
        }

        private static void modSwitchGateConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("objects/switchgate/icon"))) {
                Logger.Log("MaxHelpingHand/ReskinnableSwitchGate", $"Modding switch gate icon at {cursor.Index} in IL for SwitchGate constructor");
                cursor.EmitDelegate<Func<string, string>>(orig => iconPath != null ? iconPath : orig);
            }
        }

        public ReskinnableSwitchGate(EntityData data, Vector2 offset) : base(data, offset) {
            DynData<SwitchGate> thisData = new DynData<SwitchGate>(this);

            // replace the switch gate colors, that are conveniently stored as local variables.
            thisData["inactiveColor"] = Calc.HexToColor(data.Attr("inactiveColor"));
            thisData["activeColor"] = Calc.HexToColor(data.Attr("activeColor"));
            thisData["finishColor"] = Calc.HexToColor(data.Attr("finishColor"));

            // the initial color of the switch is inactiveColor, but we might have changed it!
            thisData.Get<Sprite>("icon").Color = thisData.Get<Color>("inactiveColor");
        }
    }
}
