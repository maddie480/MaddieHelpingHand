using MonoMod.ModInterop;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Module {
    /// <summary>
    /// Some Helping Hand entities (such as jumpthrus) only load their hooks when entering a map,
    /// if entities matching a certain list of names are present in the map.
    /// This ModInterop class allows other mods to add their own entity names to the lists.
    /// </summary>
    [ModExportName("MaddieHelpingHand/EntityIdRegistry")]
    public static class EntityNameRegistry {
        internal static HashSet<string> CustomizableGlassBlockControllers = ["MaxHelpingHand/CustomizableGlassBlockController"];
        internal static HashSet<string> KevinBarriers = ["MaxHelpingHand/KevinBarrier"];
        internal static HashSet<string> RainbowSpinnerColorControllers = ["MaxHelpingHand/FlagRainbowSpinnerColorController",
            "MaxHelpingHand/RainbowSpinnerColorController", "MaxHelpingHand/RainbowSpinnerColorControllerDisabler"];
        internal static HashSet<string> SeekerBarrierColorControllers = ["MaxHelpingHand/SeekerBarrierColorController",
            "MaxHelpingHand/SeekerBarrierColorControllerDisabler"];
        internal static HashSet<string> SidewaysJumpThrus = ["MaxHelpingHand/SidewaysJumpThru", "MaxHelpingHand/AttachedSidewaysJumpThru",
            "MaxHelpingHand/OneWayInvisibleBarrierHorizontal", "MaxHelpingHand/SidewaysMovingPlatform"];
        internal static HashSet<string> UpsideDownJumpThrus = ["MaxHelpingHand/UpsideDownJumpThru", "MaxHelpingHand/UpsideDownMovingPlatform"];

        public static void RegisterCustomizableGlassBlockController(string name) {
            CustomizableGlassBlockControllers.Add(name);
        }
        public static void RegisterKevinBarrier(string name) {
            KevinBarriers.Add(name);
        }
        public static void RegisterRainbowSpinnerColorController(string name) {
            RainbowSpinnerColorControllers.Add(name);
        }
        public static void RegisterSeekerBarrierColorController(string name) {
            SeekerBarrierColorControllers.Add(name);
        }
        public static void RegisterSidewaysJumpThru(string name) {
            SidewaysJumpThrus.Add(name);
        }
        public static void RegisterUpsideDownJumpThru(string name) {
            UpsideDownJumpThrus.Add(name);
        }
    }
}
