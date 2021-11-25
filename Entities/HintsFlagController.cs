using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/HintsFlagController")]
    public class HintsFlagController : Entity {
        private string outputFlag;
        private bool inverted;

        public HintsFlagController(EntityData data, Vector2 offset) : base() {
            outputFlag = data.Attr("outputFlag");
            inverted = data.Bool("not");
        }

        public override void Update() {
            base.Update();
            Session session = SceneAs<Level>().Session;

            bool hints = MaxHelpingHandModule.Instance.Settings.ShowHints.Check;
            if (inverted) {
                session.SetFlag(outputFlag, !hints);
            } else {
                session.SetFlag(outputFlag, hints);
            }
        }
    }
}
