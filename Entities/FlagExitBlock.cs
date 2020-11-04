using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;
using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/FlagExitBlock")]
    class FlagExitBlock : ExitBlock {
        private TileGrid tiles;
        private EffectCutout cutout;
        private readonly string flag;
        private readonly bool playSound;

        public FlagExitBlock(EntityData data, Vector2 offset) : base(data, offset) {
            flag = data.Attr("flag");
            playSound = data.Bool("playSound");

            // I'm not sure what this transition listener is for.
            Remove(Get<TransitionListener>());
        }

        // In regular C# code we can't just call the parent's base method...
        // but with MonoMod magic we can do it anyway.
        [MonoModLinkTo("Celeste.Solid", "System.Void Update()")]
        public void base_Update() {
            base.Update();
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // get some variables from the parent class.
            DynData<ExitBlock> self = new DynData<ExitBlock>(this);
            tiles = self.Get<TileGrid>("tiles");
            cutout = self.Get<EffectCutout>("cutout");

            // hide the block if the flag is initially inactive.
            if (!SceneAs<Level>().Session.GetFlag(flag)) {
                cutout.Alpha = tiles.Alpha = 0f;
                Collidable = false;
            }
        }

        public override void Update() {
            base_Update();

            bool wasCollidable = Collidable;

            // the block is only collidable if the flag is set.
            Collidable = SceneAs<Level>().Session.GetFlag(flag) && !CollideCheck<Player>();

            if (playSound && !wasCollidable && Collidable) {
                Audio.Play("event:/game/general/passage_closed_behind", base.Center);
            }

            // fade the block in or out depending on its enabled status.
            cutout.Alpha = tiles.Alpha = Calc.Approach(tiles.Alpha, Collidable ? 1f : 0f, Engine.DeltaTime);
        }

    }
}
