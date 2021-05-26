using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // A Bumper that has a Not Core Mode option.
    // Not made available as an entity directly, but used as a base class for other entities.
    public class BumperNotCoreMode : Bumper {
        public static void Load() {
            On.Celeste.Bumper.UpdatePosition += onBumperWiggle;
        }

        public static void Unload() {
            On.Celeste.Bumper.UpdatePosition -= onBumperWiggle;
        }

        private static void onBumperWiggle(On.Celeste.Bumper.orig_UpdatePosition orig, Bumper self) {
            // we handle the wobbling ourselves, so deactivate it for our bumpers.
            if (!(self is BumperNotCoreMode)) {
                orig(self);
            }
        }

        private readonly bool notCoreMode;
        private readonly bool wobble;
        protected SineWave sine;

        private Vector2 anchor;

        public BumperNotCoreMode(Vector2 position, Vector2? node, bool notCoreMode, bool wobble) : base(position, node) {
            this.notCoreMode = notCoreMode;
            this.wobble = wobble;
            sine = Get<SineWave>();

            anchor = Position;

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

        protected StaticMover MakeWobbleStaticMover() {
            if (!wobble) {
                // no wobble => just set the position right away.
                return new StaticMover();
            }

            // wobble => make the anchor move, the Update method will add it up with the wobble.
            return new StaticMover {
                OnMove = move => anchor += move
            };
        }

        public override void Update() {
            base.Update();

            if (wobble) {
                // wobble EXACTLY like vanilla, except anchor is a field in our own class.
                Position = anchor + new Vector2(sine.Value * 3f, sine.ValueOverTwo * 2f);
            }
        }
    }
}
