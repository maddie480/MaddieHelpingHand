using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// Glass Block pulled from Celeste 1.2.6.1.
    /// Usable with CustomizableGlassBlockController to get custom star/bg colors.
    /// </summary>
    [Tracked(inherited: true)]
    [CustomEntity("MaxHelpingHand/CustomizableGlassBlock")]
    public class CustomizableGlassBlock : Solid {
        public float Alpha { get; protected set; } = 1;

        public Vector2 ShakeVector { get; protected set; } = Vector2.Zero;

        private struct Line {
            public Vector2 A;
            public Vector2 B;
            public Func<float> Alpha;

            public Line(Vector2 a, Vector2 b, Func<float> alpha) {
                A = a;
                B = b;
                Alpha = alpha;
            }
        }

        private List<Line> lines = new List<Line>();
        private Color lineColor = Color.White;

        public CustomizableGlassBlock(Vector2 position, float width, float height, bool behindFgTiles)
            : base(position, width, height, safe: false) {

            Depth = behindFgTiles ? -9995 : -10000;
            Add(new LightOcclude());
            Add(new MirrorSurface());
            SurfaceSoundIndex = 32;
        }

        public CustomizableGlassBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Bool("behindFgTiles")) { }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            ComputeLines();
        }

        private void ComputeLines() {
            int widthInTiles = (int) Width / 8;
            int heightInTiles = (int) Height / 8;
            AddSide(new Vector2(0f, 0f), new Vector2(0f, -1f), widthInTiles);
            AddSide(new Vector2(widthInTiles - 1, 0f), new Vector2(1f, 0f), heightInTiles);
            AddSide(new Vector2(widthInTiles - 1, heightInTiles - 1), new Vector2(0f, 1f), widthInTiles);
            AddSide(new Vector2(0f, heightInTiles - 1), new Vector2(-1f, 0f), heightInTiles);
        }

        private void AddSide(Vector2 start, Vector2 normal, int tiles) {
            Vector2 vector = new Vector2(0f - normal.Y, normal.X);
            for (int i = 0; i < tiles; i++) {
                if (Open(start + vector * i + normal)) {
                    Vector2 a = (start + vector * i) * 8f + new Vector2(4f) - vector * 4f + normal * 4f;
                    if (!Open(start + vector * (i - 1))) {
                        a -= vector;
                    }
                    for (; i < tiles && Open(start + vector * i + normal); i++) { }

                    Vector2 b = (start + vector * i) * 8f + new Vector2(4f) - vector * 4f + normal * 4f;
                    if (!Open(start + vector * i)) {
                        b += vector;
                    }

                    lines.Add(new Line(a, b, () => 1));
                }
            }
        }

        private bool Open(Vector2 tile) {
            if (this is CustomizableGlassFallingBlock) return true;

            Vector2 point = new Vector2(X + tile.X * 8f + 4f, Y + tile.Y * 8f + 4f);
            if (!Scene.CollideCheck<SolidTiles>(point)) {
                CustomizableGlassBlock block = Scene.CollideFirst<CustomizableGlassBlock>(point);
                return block == null || block is CustomizableGlassFallingBlock;
            }
            return false;
        }

        public override void Render() {
            foreach (Line line in lines) {
                Draw.Line(Position + ShakeVector + line.A, Position + ShakeVector + line.B, lineColor * line.Alpha() * Alpha);
            }
        }

        internal void GlassExitBlockSolidified(CustomizableGlassExitBlock exitBlock) {
            // save current lines
            List<Line> oldLines = lines;

            // recompute all lines
            lines = new List<Line>();
            ComputeLines();

            // make new lines fade in
            for (int i = 0; i < lines.Count; i++) {
                Line line = lines[i];
                line.Alpha = () => exitBlock.Alpha;
                lines[i] = line;
            }

            // make old lines fade out
            foreach (Line oldLine in oldLines) {
                Line lineFadeOut = oldLine;
                lineFadeOut.Alpha = () => 1 - exitBlock.Alpha;
                lines.Add(lineFadeOut);
            }
        }
    }
}
