using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/FlagLogicGate")]
    class FlagLogicGate : Entity {
        private string outputFlag;
        private readonly Func<Session, bool> function;

        public FlagLogicGate(EntityData data, Vector2 offset) : base() {
            outputFlag = data.Attr("outputFlag");

            // parse all input flags
            List<Func<Session, bool>> inputFlags = new List<Func<Session, bool>>();
            foreach (string inputFlag in data.Attr("inputFlags").Split(',')) {
                if (inputFlag.StartsWith("!")) {
                    string flagName = inputFlag.Substring(1);
                    inputFlags.Add(session => !session.GetFlag(flagName));
                } else {
                    inputFlags.Add(session => session.GetFlag(inputFlag));
                }
            }

            // combine them with the chosen function
            switch (data.Attr("func")) {
                case "AND":
                default:
                    function = session => inputFlags.All(flag => flag(session));
                    break;
                case "OR":
                    function = session => inputFlags.Any(flag => flag(session));
                    break;
                case "XOR":
                    function = session => inputFlags.Count(flag => flag(session)) == 1;
                    break;
            }

            // invert if needed
            if (data.Bool("not")) {
                Func<Session, bool> origFunc = function;
                function = session => !origFunc(session);
            }
        }

        public override void Update() {
            base.Update();

            Session session = SceneAs<Level>().Session;
            session.SetFlag(outputFlag, function(session));
        }
    }
}
