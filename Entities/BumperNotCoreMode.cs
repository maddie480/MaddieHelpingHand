using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // A Bumper that has a Not Core Mode option.
    // Not made available as an entity directly, but used as a base class for other entities.
    class BumperNotCoreMode : Bumper {
        private readonly bool notCoreMode;

        public BumperNotCoreMode(Vector2 position, Vector2? node, bool notCoreMode) : base(position, node) {
            this.notCoreMode = notCoreMode;

            if (notCoreMode) {
                // make this bumper insensitive to core mode.
                Remove(Get<CoreModeListener>());
            }
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (notCoreMode) {
                // revert the activation of fire mode bumper.
                DynData<Bumper> thisBumper = new DynData<Bumper>(this);
                thisBumper["fireMode"] = false;
                thisBumper.Get<Sprite>("spriteEvil").Visible = false;
                thisBumper.Get<Sprite>("sprite").Visible = true;
            }
        }
    }
}
