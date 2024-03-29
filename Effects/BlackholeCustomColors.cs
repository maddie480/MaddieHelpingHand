﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class BlackholeCustomColors : BlackholeBG {
        private static Color[] colorsMild;
        private static List<MTexture> replacementAtlasSubtextures;

        public static void Load() {
            IL.Celeste.BlackholeBG.ctor += onBlackholeConstructor;
            IL.Celeste.BlackholeBG.Update += modBlackholeUpdate;
            IL.Celeste.BlackholeBG.BeforeRender += modBlackholeBeforeRender;
        }

        public static void Unload() {
            IL.Celeste.BlackholeBG.ctor -= onBlackholeConstructor;
            IL.Celeste.BlackholeBG.Update -= modBlackholeUpdate;
            IL.Celeste.BlackholeBG.BeforeRender -= modBlackholeBeforeRender;
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

            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdstr("bgs/10/blackhole/particle"),
                instr => instr.MatchCallvirt<Atlas>("GetAtlasSubtextures"))) {

                Logger.Log("MaxHelpingHand/BlackholeCustomColors", $"Replacing particle textures at {cursor.Index} in IL code for BlackholeBG constructor");
                cursor.EmitDelegate<Func<List<MTexture>, List<MTexture>>>(orig => {
                    if (replacementAtlasSubtextures != null) {
                        List<MTexture> result = replacementAtlasSubtextures;
                        replacementAtlasSubtextures = null;
                        return result;
                    }
                    return orig;
                });
            }
        }

        public static BlackholeBG CreateBlackholeWithCustomColors(BinaryPacker.Element effectData) {
            float bgAlphaInner = effectData.AttrFloat("bgAlphaInner", 1f);
            float bgAlphaOuter = effectData.AttrFloat("bgAlphaOuter", 1f);

            // backwards compat for an attribute that only existed in 1.21.8, that was the latest version for a whole 1 day
            if (effectData.HasAttr("bgAlpha")) {
                bgAlphaInner = effectData.AttrFloat("bgAlpha", 1f);
                bgAlphaOuter = effectData.AttrFloat("bgAlpha", 1f);
            }

            if (!effectData.Attr("colorsMild").Contains("|")
                && !effectData.Attr("colorsWild").Contains("|")
                && !effectData.Attr("bgColorInner").Contains("|")
                && !effectData.Attr("bgColorOuterMild").Contains("|")
                && !effectData.Attr("bgColorOuterWild").Contains("|")
                && effectData.AttrBool("affectedByWind", true)
                && effectData.AttrFloat("additionalWindX", 0f) == 0f
                && effectData.AttrFloat("additionalWindY", 0f) == 0f
                && effectData.AttrFloat("fgAlpha", 1f) == 1f
                && string.IsNullOrEmpty(effectData.Attr("fadex"))
                && string.IsNullOrEmpty(effectData.Attr("fadey"))
                && !effectData.AttrBool("invertedRendering")
                && string.IsNullOrEmpty(effectData.Attr("particleTexture"))) {

                // there is no gradient on any color, wind should affect the blackhole, and there is no fade: we can just instanciate a vanilla blackhole and mess with its properties.

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
                blackholeData["bgColorInner"] = Calc.HexToColor(effectData.Attr("bgColorInner", "000000")) * bgAlphaInner;
                blackholeData["bgColorOuterMild"] = Calc.HexToColor(effectData.Attr("bgColorOuterMild", "512a8b")) * bgAlphaOuter;
                blackholeData["bgColorOuterWild"] = Calc.HexToColor(effectData.Attr("bgColorOuterWild", "bd2192")) * bgAlphaOuter;
                blackhole.Alpha = effectData.AttrFloat("alpha", 1f);
                blackhole.Direction = effectData.AttrFloat("direction", 1f);

                if (!string.IsNullOrEmpty(effectData.Attr("texture"))) {
                    blackholeData["bgTexture"] = GFX.Game[effectData.Attr("texture")];
                }

                return blackhole;
            } else {
                // there are gradients, or the blackhole should not be affected by wind: we need a custom blackhole!

                float fgAlpha = effectData.AttrFloat("fgAlpha", 1f);

                MTexture particleTexture = null;
                if (!string.IsNullOrEmpty(effectData.Attr("particleTexture"))) {
                    particleTexture = GFX.Game[effectData.Attr("particleTexture")];
                    replacementAtlasSubtextures = cutIntoSubtextures(particleTexture, effectData.AttrInt("particleTextureCount"));
                }

                // set up colorsMild for the hook above. we can't use DynData to pass this over, since the object does not exist yet!
                colorsMild = new ColorCycle(effectData.Attr("colorsMild", "6e3199,851f91,3026b0"), 0.8f * fgAlpha).GetColors();

                // build the blackhole: the hook will take care of setting colorsMild.
                BlackholeCustomColors blackhole = new BlackholeCustomColors(
                    effectData.Attr("colorsMild", "6e3199,851f91,3026b0"),
                    effectData.Attr("colorsWild", "ca4ca7,b14cca,ca4ca7"),
                    effectData.Attr("bgColorInner", "000000"),
                    effectData.Attr("bgColorOuterMild", "512a8b"),
                    effectData.Attr("bgColorOuterWild", "bd2192"),
                    bgAlphaInner, bgAlphaOuter, fgAlpha,
                    effectData.AttrBool("affectedByWind", true),
                    new Vector2(effectData.AttrFloat("additionalWindX", 0f), effectData.AttrFloat("additionalWindY", 0f)),
                    effectData.AttrBool("invertedRendering"),
                    particleTexture);

                // ... now we've got to set the initial values of everything else.
                blackhole.blackholeData["colorsWild"] = blackhole.cycleColorsWild.GetColors();
                blackhole.blackholeData["bgColorInner"] = blackhole.cycleBgColorInner.GetColors()[0];
                blackhole.blackholeData["bgColorOuterMild"] = blackhole.cycleBgColorOuterMild.GetColors()[0];
                blackhole.blackholeData["bgColorOuterWild"] = blackhole.cycleBgColorOuterWild.GetColors()[0];
                blackhole.Alpha = effectData.AttrFloat("alpha", 1f);
                blackhole.Direction = effectData.AttrFloat("direction", 1f);

                if (!string.IsNullOrEmpty(effectData.Attr("texture"))) {
                    blackhole.blackholeData["bgTexture"] = GFX.Game[effectData.Attr("texture")];
                }

                return blackhole;
            }
        }

        /// <summary>
        /// Cuts a texture evenly into the given amount of subtextures.
        /// </summary>
        private static List<MTexture> cutIntoSubtextures(MTexture texture, int subtextureCount) {
            List<MTexture> result = new List<MTexture>();

            int sectionWidth = texture.Width / subtextureCount;
            int currentX = 0;

            for (int i = 0; i < subtextureCount; i++) {
                result.Add(new MTexture(texture, new Rectangle(currentX, 0, sectionWidth, texture.Height)));
                currentX += sectionWidth;
            }

            return result;
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

            public ColorCycle(string src, float alpha) {
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
                        colors[i][j] *= alpha;
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
        private readonly Vector2 additionalWind;

        private readonly float fgAlpha;
        private readonly bool invertedRendering;

        private readonly MTexture particleTexture;

        public BlackholeCustomColors(string colorsMild, string colorsWild, string bgColorInner, string bgColorOuterMild, string bgColorOuterWild,
            float bgAlphaInner, float bgAlphaOuter, float fgAlpha, bool affectedByWind, Vector2 additionalWind, bool invertedRendering, MTexture particleTexture) : base() {

            blackholeData = new DynData<BlackholeBG>(this);

            // parse all color cycles.
            cycleColorsMild = new ColorCycle(colorsMild, 0.8f * fgAlpha);
            cycleColorsWild = new ColorCycle(colorsWild, fgAlpha);
            cycleBgColorInner = new ColorCycle(bgColorInner, bgAlphaInner);
            cycleBgColorOuterMild = new ColorCycle(bgColorOuterMild, bgAlphaOuter);
            cycleBgColorOuterWild = new ColorCycle(bgColorOuterWild, bgAlphaOuter);

            this.affectedByWind = affectedByWind;
            this.additionalWind = additionalWind;

            this.particleTexture = particleTexture;

            this.fgAlpha = fgAlpha;
            this.invertedRendering = invertedRendering;

            if (invertedRendering) {
                blackholeData["bgTexture"] = GFX.Game["objects/MaxHelpingHand/temple/portal_inverted"];
            }
        }

        public override void Update(Scene scene) {
            if (affectedByWind && additionalWind == Vector2.Zero) {
                base.Update(scene);
            } else {
                Level level = scene as Level;

                // remove and/or modify wind
                Vector2 bakWind = level.Wind;
                if (!affectedByWind) {
                    level.Wind = Vector2.Zero;
                }
                level.Wind += additionalWind;

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

        public override void Render(Scene scene) {
            float origAlpha = Alpha;
            Alpha *= CustomBackdrop.GetFadeAlphaFor(this, scene);

            base.Render(scene);

            Alpha = origAlpha;
        }

        private static void modBlackholeUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            replacerHook<Color>(cursor, cursor => cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Color>("get_Black")), 0, (orig, self) => orig * self.fgAlpha);
        }

        private static void modBlackholeBeforeRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // if inverted is enabled, reverse the loop: for (int i = 19; 19 - i < 20; i--), 19 - i < 20 => i > -1 => i >= 0
            replacerHook<int>(cursor, cursor => cursor.TryGotoNext(instr => instr.MatchLdcI4(0), instr => instr.MatchStloc(1)),
                1, (orig, self) => self.invertedRendering ? 19 : orig);
            replacerHook<int>(cursor, cursor => cursor.TryGotoNext(instr => instr.MatchLdcI4(1), instr => instr.MatchAdd(), instr => instr.MatchStloc(1)),
                1, (orig, self) => self.invertedRendering ? -1 : orig);
            replacerHook<int>(cursor, cursor => cursor.TryGotoNext(instr => instr.MatchLdloc(1), instr => instr.MatchLdcI4(20), instr => instr.OpCode == OpCodes.Blt),
                1, (orig, self) => self.invertedRendering ? 19 - orig : orig);
            replacerHook<Color>(cursor, cursor => cursor.TryGotoNext(instr => instr.MatchLdfld<BlackholeBG>("bgColorInner"), instr => instr.MatchCallvirt<GraphicsDevice>("Clear")),
                1, (orig, self) => self.invertedRendering ? Color.Transparent : orig);

            // replace the particle texture if any was provided
            replacerHook<Texture2D>(cursor, cursor => cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<VirtualTexture>("get_Texture_Safe")),
                0, (orig, self) => self.particleTexture != null ? self.particleTexture.Texture.Texture : orig);
        }

        /**
         * The given condition should move the cursor after a method returning a T (optionally using the offset to do that).
         * Then, if "this" is a BlackholeCustomColors, the return value of the method will be replaced with what replaceWith returns.
         */
        private static void replacerHook<T>(ILCursor cursor, Func<ILCursor, bool> condition, int offset, Func<T, BlackholeCustomColors, T> replaceWith) {
            while (condition(cursor)) {
                Logger.Log("MaxHelpingHand/BlackholeCustomColors", $"Applying patch in {cursor.Index} in IL for {cursor.Method.FullName}");

                cursor.Index += offset;

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<T, BlackholeBG, T>>((orig, self) => {
                    if (self is BlackholeCustomColors blackhole) {
                        return replaceWith(orig, blackhole);
                    }
                    return orig;
                });
            }

            cursor.Index = 0;
        }
    }
}
