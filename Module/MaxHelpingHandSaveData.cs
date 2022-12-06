using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandSaveData : EverestModuleSaveData {
        // map SID => list of opened strawberry gate entity IDs
        public Dictionary<string, HashSet<int>> OpenedSaveFileStrawberryGates { get; set; } = new Dictionary<string, HashSet<int>>();
    }
}
