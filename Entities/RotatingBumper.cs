using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/RotatingBumper")]
    public class RotatingBumper : BumperNotCoreMode {
        private Vector2 center;
        private readonly float radius;
        private readonly float speed; // in rad/s
        private readonly bool wobble;

        private float currentAngle;

        public RotatingBumper(EntityData data, Vector2 offset) : base(data.Position + offset, null, data.Bool("notCoreMode", false), wobble: false /* we handle wobbling ourselves */) {
            Vector2 startingPosition = Position;

            center = data.NodesOffset(offset)[0];
            radius = (center - startingPosition).Length();
            speed = (float) (data.Float("speed", 360) * Math.PI / 180D);
            wobble = data.Bool("wobble", false);

            currentAngle = Calc.WrapAngle(Calc.Angle(center, startingPosition));

            if (data.Bool("attachToCenter")) {
                // attach center to any entity that is there.
                Add(new StaticMover {
                    SolidChecker = solid => CollideCheck(solid, center),
                    JumpThruChecker = jumpThru => CollideCheck(jumpThru, center),
                    OnMove = amount => center += amount
                });
            }
        }

        public override void Update() {
            base.Update();

            currentAngle += Engine.DeltaTime * speed;
            currentAngle = Calc.WrapAngle(currentAngle);

            Position = center + Calc.AngleToVector(currentAngle, radius);

            if (wobble) {
                Position += new Vector2(sine.Value * 3f, sine.ValueOverTwo * 2f);
            }
        }
    }
}
