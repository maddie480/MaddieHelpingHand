using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    class CustomStars : StarsBG {
        internal static string StarsDirectory;
        internal static bool TintingDisabled;

        public static void Load() {
            IL.Celeste.StarsBG.ctor += modStarsBGConstructor;
            IL.Celeste.StarsBG.Render += modStarsBGRender;
        }

        public static void Unload() {
            IL.Celeste.StarsBG.ctor -= modStarsBGConstructor;
            IL.Celeste.StarsBG.Render -= modStarsBGRender;
        }

        private static void modStarsBGConstructor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // inject ourselves right after the star textures are loaded, and right before they are used.
            if (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdstr("bgs/02/stars/c"),
                instr => instr.MatchCallvirt<Atlas>("GetAtlasSubtextures"),
                instr => instr.OpCode == OpCodes.Callvirt)) {

                Logger.Log("MaxHelpingHand/CustomStars", $"Injecting call to customize stars at {cursor.Index} in IL for the StarsBG constructor");
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(StarsBG).GetField("textures", BindingFlags.NonPublic | BindingFlags.Instance));
                cursor.EmitDelegate<Action<StarsBG, List<List<MTexture>>>>((self, textures) => {
                    if (self is CustomStars) {
                        textures.Clear();

                        // look up all the stars in the folder, and group frames belonging to the same stars.
                        List<MTexture> folderContents = GFX.Game.Textures.Keys.Where(path => path.StartsWith(StarsDirectory + "/")).Select(path => GFX.Game[path]).ToList();
                        Dictionary<string, List<MTexture>> starList = new Dictionary<string, List<MTexture>>();
                        foreach (MTexture texture in folderContents) {
                            string starName = Regex.Replace(texture.AtlasPath, "\\d+$", string.Empty);
                            if (!starList.TryGetValue(starName, out List<MTexture> starTextures)) {
                                starTextures = new List<MTexture>();
                                starList[starName] = starTextures;
                            }
                            starTextures.Add(texture);
                        }

                        // be sure that the star sprites are ordered.
                        foreach (List<MTexture> list in starList.Values) {
                            list.Sort((a, b) => a.AtlasPath.CompareTo(b.AtlasPath));
                        }

                        // then add tlem to the texture list for the game to use them.
                        textures.AddRange(starList.Values);

                        StarsDirectory = null;
                    }
                });
            }

            // patch the teal tinting of all colors to make it white instead.
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.7f), instr => instr.MatchCall<Color>("op_Multiply"))) {
                Logger.Log("MaxHelpingHand/CustomStars", $"Injecting call to disable tinting at {cursor.Index} in IL for the StarsBG constructor");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Color, StarsBG, Color>>((orig, self) => {
                    if (self is CustomStars && TintingDisabled) {
                        return Color.White * 0.7f;
                    }
                    return orig;
                });
            }
        }

        private static void modStarsBGRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // patch the teal tinting of all colors to make it white instead.
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.7f), instr => instr.MatchCall<Color>("op_Multiply"))) {
                Logger.Log("MaxHelpingHand/CustomStars", $"Injecting call to disable tinting at {cursor.Index} in IL for StarsBG.Render");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<Color, StarsBG, Color>>((orig, self) => {
                    if (self is CustomStars customStars && customStars.tintingDisabled) {
                        return Color.White * 0.7f;
                    }
                    return orig;
                });
            }
        }

        private bool tintingDisabled;

        public CustomStars(bool tintingDisabled) : base() {
            this.tintingDisabled = tintingDisabled;
        }
    }
}
