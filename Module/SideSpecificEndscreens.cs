using Celeste.Mod.Meta;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Module {
    /**
     * Allows mappers to define different endscreens for each side in a chapter's meta.yaml file.
     * Originally made for D-sides, and as such, also ships with D-Sides Helper.
     */
    public static class SideSpecificEndscreens {
        private class MetaModel {
            public Dictionary<AreaMode, MapMetaCompleteScreen> CompleteScreensBySide { get; set; }
        }

        public static void Load() {
            IL.Celeste.LevelExit.LoadCompleteThread += ilSwapAreaCompleteScreen;
        }

        public static void Unload() {
            IL.Celeste.LevelExit.LoadCompleteThread -= ilSwapAreaCompleteScreen;
        }

        private static void ilSwapAreaCompleteScreen(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<MapMeta>("get_CompleteScreen"));
            Logger.Log(LogLevel.Verbose, "MaxHelpingHand/SideSpecificEndscreens", $"Injecting map complete screen swapping code at {cursor.Index} in IL for LevelExit.LoadCompleteThread");
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, typeof(LevelExit).GetField("session", BindingFlags.NonPublic | BindingFlags.Instance));
            cursor.EmitDelegate(swapAreaCompleteScreen);
        }

        private static MapMetaCompleteScreen swapAreaCompleteScreen(MapMetaCompleteScreen orig, Session session) {
            if (Everest.Content.Map.TryGetValue("Maps/" + session.Area.SID, out ModAsset asset)
                && asset.TryGetMeta(out MetaModel metaModel)
                && metaModel?.CompleteScreensBySide != null
                && metaModel.CompleteScreensBySide.TryGetValue(session.Area.Mode, out MapMetaCompleteScreen completeScreen)) {

                Logger.Log(LogLevel.Info, "MaxHelpingHand/SideSpecificEndscreens", $"Overriding endscreen for {session.Area.SID} / {session.Area.Mode}");
                return completeScreen;
            }

            return orig;
        }
    }
}
