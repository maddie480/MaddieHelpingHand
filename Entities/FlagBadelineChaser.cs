using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/FlagBadelineChaser")]
    [TrackedAs(typeof(BadelineOldsite))]
    class FlagBadelineChaser : BadelineOldsite {
        private readonly string flagName;
        private bool started;

        public FlagBadelineChaser(EntityData data, Vector2 offset) : base(data, offset, data.Int("index")) {
            flagName = data.Attr("flag");
        }

        public override void Update() {
            // only update if the chaser started chasing
            if (started || SceneAs<Level>().Session.GetFlag(flagName)) {
                base.Update();
                started = true;
            }

            if (started && !SceneAs<Level>().Session.GetFlag(flagName)) {
                // poof out
                Level level = Scene as Level;
                Audio.Play("event:/char/badeline/disappear", Position);
                level.Displacement.AddBurst(Center, 0.5f, 24f, 96f, 0.4f);
                level.Particles.Emit(P_Vanish, 12, Center, Vector2.One * 6f);
                RemoveSelf();
            }
        }

    }
}
