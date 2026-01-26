using MonoMod.ModInterop;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public static class LuckyHelperImports {
        [ModImportName("LuckyHelper")]
        public static class Interop
        {
            public static Func<List<Player>> GetDummyPlayers;
        }

        public static List<Player> GetDummyPlayers() => Interop.GetDummyPlayers?.Invoke() ?? new List<Player>();
    }
}