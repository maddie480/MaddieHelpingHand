using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomMemorialWithDreamingAttribute")]
    class CustomMemorialWithDreamingAttribute : CustomMemorial {
        private bool dreaming;

        public CustomMemorialWithDreamingAttribute(EntityData data, Vector2 offset) : base(data, offset) {
            dreaming = data.Bool("dreaming");
        }

        public override void Added(Scene scene) {
            Session session = (scene as Level).Session;
            bool oldDreaming = session.Dreaming;
            session.Dreaming = dreaming;

            base.Added(scene);

            session.Dreaming = oldDreaming;
        }

        public override void Update() {
            Session session = SceneAs<Level>().Session;
            bool oldDreaming = session.Dreaming;
            session.Dreaming = dreaming;

            base.Update();

            session.Dreaming = oldDreaming;
        }
    }
}
