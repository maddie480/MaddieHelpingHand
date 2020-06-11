using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    static class BlackholeCustomColors {
        private static Color[] colorsMild;

        public static void Load() {
            IL.Celeste.BlackholeBG.ctor += onBlackholeConstructor;
        }

        public static void Unload() {
            IL.Celeste.BlackholeBG.ctor -= onBlackholeConstructor;
        }

        private static void onBlackholeConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // we want to insert code at the beginning of the constructor, but after the variable initialization code,
            // because colorsMild is used right in the constructor.
            // the "first line" of the constructor is: bgTexture = GFX.Game["objects/temple/portal/portal"];
            if (cursor.TryGotoNext(MoveType.Before,
                instr => instr.MatchLdarg(0),
                instr => instr.MatchLdsfld(typeof(GFX), "Game"))) {

                Logger.Log("MaxHelpingHand/BlackholeCustomColors", $"Replacing colorsMild at {cursor.Index} in IL code for BlackholeBG constructor");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Action<BlackholeBG>>(self => {
                    if (colorsMild != null) {
                        new DynData<BlackholeBG>(self)["colorsMild"] = colorsMild;
                        colorsMild = null;
                    }
                });
            }
        }

        public static BlackholeBG CreateBlackholeWithCustomColors(BinaryPacker.Element effectData) {
            // set up colorsMild for the hook above. we can't use DynData to pass this over, since the object does not exist yet!
            colorsMild = parseColors(effectData.Attr("colorsMild", "6e3199,851f91,3026b0"));
            for (int i = 0; i < colorsMild.Length; i++) {
                colorsMild[i] *= 0.8f;
            }

            // build the blackhole: the hook will take care of setting colorsMild.
            BlackholeBG blackhole = new BlackholeBG();

            // ... now we've got to set everything else.
            DynData<BlackholeBG> blackholeData = new DynData<BlackholeBG>(blackhole);
            blackholeData["colorsWild"] = parseColors(effectData.Attr("colorsWild", "ca4ca7,b14cca,ca4ca7"));
            blackholeData["bgColorInner"] = Calc.HexToColor(effectData.Attr("bgColorInner", "000000"));
            blackholeData["bgColorOuterMild"] = Calc.HexToColor(effectData.Attr("bgColorOuterMild", "512a8b"));
            blackholeData["bgColorOuterWild"] = Calc.HexToColor(effectData.Attr("bgColorOuterWild", "bd2192"));

            return blackhole;
        }

        private static Color[] parseColors(string input) {
            string[] colorsAsStrings = input.Split(',');
            Color[] colors = new Color[colorsAsStrings.Length];
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = Calc.HexToColor(colorsAsStrings[i]);
            }
            return colors;
        }
    }
}
