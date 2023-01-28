using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // For those that want to reskin their touch switches without all the flag stuff.
    [CustomEntity("MaxHelpingHand/ReskinnableTouchSwitch")]
    [TrackedAs(typeof(TouchSwitch))]
    public class ReskinnableTouchSwitch : TouchSwitch {
        private static string iconPath = null;

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            // we replace the icon using an IL hook, because a fair bit of setup happens in the constructor, and replacing the icon after that is less convenient.
            iconPath = entityData.Attr("icon");
            Entity result = new ReskinnableTouchSwitch(entityData, offset);
            iconPath = null;
            return result;
        }

        public static void Load() {
            IL.Celeste.TouchSwitch.ctor_Vector2 += modTouchSwitchConstructor;
        }

        public static void Unload() {
            IL.Celeste.TouchSwitch.ctor_Vector2 -= modTouchSwitchConstructor;
        }

        private static void modTouchSwitchConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("objects/touchswitch/icon"))) {
                Logger.Log("MaxHelpingHand/ReskinnableTouchSwitch", $"Modding touch switch icon at {cursor.Index} in IL for TouchSwitch constructor");
                cursor.EmitDelegate<Func<string, string>>(orig => iconPath != null ? iconPath : orig);
            }
        }

        public ReskinnableTouchSwitch(EntityData data, Vector2 offset) : base(data, offset) {
            DynData<TouchSwitch> thisData = new DynData<TouchSwitch>(this);

            // replace the other components of the touch switch, that are conveniently stored as local variables.
            thisData["border"] = GFX.Game[data.Attr("borderTexture")];
            thisData["inactiveColor"] = Calc.HexToColor(data.Attr("inactiveColor"));
            thisData["activeColor"] = Calc.HexToColor(data.Attr("activeColor"));
            thisData["finishColor"] = Calc.HexToColor(data.Attr("finishColor"));

            // the initial color of the switch is inactiveColor, but we might have changed it!
            thisData.Get<Sprite>("icon").Color = thisData.Get<Color>("inactiveColor");
        }
    }
}
