using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class BlackholeCustomColors : BlackholeBG {
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
            if (!effectData.Attr("colorsMild").Contains("|")
                && !effectData.Attr("colorsWild").Contains("|")
                && !effectData.Attr("bgColorInner").Contains("|")
                && !effectData.Attr("bgColorOuterMild").Contains("|")
                && !effectData.Attr("bgColorOuterWild").Contains("|")
                && effectData.AttrBool("affectedByWind", true)) {

                // there is no gradient on any color, and wind should affect the blackhole: we can just instanciate a vanilla blackhole and mess with its properties.

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
                blackhole.Alpha = effectData.AttrFloat("alpha", 1f);

                return blackhole;
            } else {
                // there are gradients, or the blackhole should not be affected by wind: we need a custom blackhole!

                // set up colorsMild for the hook above. we can't use DynData to pass this over, since the object does not exist yet!
                colorsMild = new ColorCycle(effectData.Attr("colorsMild", "6e3199,851f91,3026b0"), 0.8f).GetColors();

                // build the blackhole: the hook will take care of setting colorsMild.
                BlackholeCustomColors blackhole = new BlackholeCustomColors(
                    effectData.Attr("colorsMild", "6e3199,851f91,3026b0"),
                    effectData.Attr("colorsWild", "ca4ca7,b14cca,ca4ca7"),
                    effectData.Attr("bgColorInner", "000000"),
                    effectData.Attr("bgColorOuterMild", "512a8b"),
                    effectData.Attr("bgColorOuterWild", "bd2192"),
                    effectData.AttrBool("affectedByWind", true));

                // ... now we've got to set the initial values of everything else.
                blackhole.blackholeData["colorsWild"] = blackhole.cycleColorsWild.GetColors();
                blackhole.blackholeData["bgColorInner"] = blackhole.cycleBgColorInner.GetColors()[0];
                blackhole.blackholeData["bgColorOuterMild"] = blackhole.cycleBgColorOuterMild.GetColors()[0];
                blackhole.blackholeData["bgColorOuterWild"] = blackhole.cycleBgColorOuterWild.GetColors()[0];
                blackhole.Alpha = effectData.AttrFloat("alpha", 1f);

                return blackhole;
            }
        }

        private static Color[] parseColors(string input) {
            string[] colorsAsStrings = input.Split(',');
            Color[] colors = new Color[colorsAsStrings.Length];
            for (int i = 0; i < colors.Length; i++) {
                colors[i] = Calc.HexToColor(colorsAsStrings[i]);
            }
            return colors;
        }

        // === gradient blackhole code

        private class ColorCycle {
            private Color[][] colors;
            private float cycle;
            private int cycleProgress;
            private float cycleTimer;

            public ColorCycle(string src, float multiplier = 1f) {
                if (!src.Contains("|")) {
                    // there is only 1 set of colors, parse it.
                    colors = new Color[][] { parseColors(src) };
                } else {
                    // the format is: 0.5|color1|color2|etc...
                    string[] srcs = src.Split('|');

                    // parse the cycle time
                    cycle = float.Parse(srcs[0]);

                    // parse the color lists
                    colors = new Color[srcs.Length - 1][];
                    for (int i = 1; i < srcs.Length; i++) {
                        colors[i - 1] = parseColors(srcs[i]);
                    }
                }

                // multiply colors with the given multiplier.
                for (int i = 0; i < colors.Length; i++) {
                    for (int j = 0; j < colors[0].Length; j++) {
                        colors[i][j] *= multiplier;
                    }
                }
            }

            public void Update() {
                // if there is only 1 set of colors, there is no cycle at all.
                if (colors.Length == 1) return;

                cycleTimer += Engine.DeltaTime;
                if (cycleTimer >= cycle) {
                    cycleTimer -= cycle;
                    cycleProgress++;
                    cycleProgress %= colors.Length;
                }
            }

            public Color[] GetColors() {
                // if there is only 1 set of colors, just return it.
                if (colors.Length == 1) return colors[0];

                Color[] colors1 = colors[cycleProgress];
                Color[] colors2 = colors[(cycleProgress + 1) % colors.Length];

                // lerp all colors 1 by 1.
                Color[] result = new Color[colors1.Length];
                for (int i = 0; i < result.Length; i++) {
                    result[i] = Color.Lerp(colors1[i], colors2[i], cycleTimer / cycle);
                }
                return result;
            }
        }

        private readonly DynData<BlackholeBG> blackholeData;

        private readonly ColorCycle cycleColorsMild;
        private readonly ColorCycle cycleColorsWild;
        private readonly ColorCycle cycleBgColorInner;
        private readonly ColorCycle cycleBgColorOuterMild;
        private readonly ColorCycle cycleBgColorOuterWild;

        private readonly bool affectedByWind;

        public BlackholeCustomColors(string colorsMild, string colorsWild, string bgColorInner, string bgColorOuterMild, string bgColorOuterWild, bool affectedByWind) : base() {
            blackholeData = new DynData<BlackholeBG>(this);

            // parse all color cycles.
            cycleColorsMild = new ColorCycle(colorsMild, 0.8f);
            cycleColorsWild = new ColorCycle(colorsWild);
            cycleBgColorInner = new ColorCycle(bgColorInner);
            cycleBgColorOuterMild = new ColorCycle(bgColorOuterMild);
            cycleBgColorOuterWild = new ColorCycle(bgColorOuterWild);

            this.affectedByWind = affectedByWind;
        }

        public override void Update(Scene scene) {
            if (affectedByWind) {
                base.Update(scene);
            } else {
                Level level = scene as Level;

                // remove wind
                Vector2 bakWind = level.Wind;
                level.Wind = Vector2.Zero;

                base.Update(scene);

                // restore wind
                level.Wind = bakWind;
            }

            // update all color cycles...
            cycleColorsMild.Update();
            cycleColorsWild.Update();
            cycleBgColorInner.Update();
            cycleBgColorOuterMild.Update();
            cycleBgColorOuterWild.Update();

            // ... and apply them.
            blackholeData["colorsMild"] = cycleColorsMild.GetColors();
            blackholeData["colorsWild"] = cycleColorsWild.GetColors();
            blackholeData["bgColorInner"] = cycleBgColorInner.GetColors()[0];
            blackholeData["bgColorOuterMild"] = cycleBgColorOuterMild.GetColors()[0];
            blackholeData["bgColorOuterWild"] = cycleBgColorOuterWild.GetColors()[0];
        }
    }
}
