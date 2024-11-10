using MonoMod.ModInterop;
using System;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public static class GravityHelperImports {
        [ModImportName("GravityHelper")]
        public static class Interop {
            public static Func<bool> IsPlayerInverted;
        }

        public static bool IsPlayerInverted() => Interop.IsPlayerInverted?.Invoke() ?? false;
    }
}