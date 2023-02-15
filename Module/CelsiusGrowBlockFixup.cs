using Celeste.Mod.Helpers;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Module {
    // This contains a hook that fixes Grow Blocks being invisible after the Everest update that added tile rendering culling.
    // Grow Blocks are present (and nearly identical) in Celsius and Viv's Helper.
    public static class CelsiusGrowBlockFixup {
        private static Hook celsiusHook;
        private static Hook vivHelperHook;

        public static void LoadMods() {
            MethodInfo hookTarget = typeof(CelsiusGrowBlockFixup).GetMethod("fixupGrowBlockTileGrid", BindingFlags.NonPublic | BindingFlags.Static);

            MethodInfo celsiusGrowBlockAwake = FakeAssembly.GetFakeEntryAssembly().GetType("Celeste.Mod.CelsiusHelper.GrowBlock")?.GetMethod("Awake");
            if (celsiusGrowBlockAwake != null) celsiusHook = new Hook(celsiusGrowBlockAwake, hookTarget);

            MethodInfo vivHelperGrowBlockAwake = FakeAssembly.GetFakeEntryAssembly().GetType("VivHelper.Entities.GrowBlock")?.GetMethod("Awake");
            if (vivHelperGrowBlockAwake != null) vivHelperHook = new Hook(vivHelperGrowBlockAwake, hookTarget);
        }

        public static void Unload() {
            celsiusHook?.Dispose();
            celsiusHook = null;

            vivHelperHook?.Dispose();
            vivHelperHook = null;
        }

        private static void fixupGrowBlockTileGrid(Action<Solid, Scene> orig, Solid self, Scene scene) {
            orig(self, scene);

            TileGrid tileGrid = self.Get<TileGrid>();

            // do not use the level camera for rendering clipping, use the block's bounds instead to be sure we render the whole block
            tileGrid.ClipCamera = new Camera(tileGrid.TileWidth * 8, tileGrid.TileHeight * 8) {
                Position = self.Position
            };
        }
    }
}
