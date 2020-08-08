using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// This is literally SeekerBarrier but Kevin.
    /// </summary>
    [Tracked]
    public class KevinBarrierRenderer : Entity {
        private class Edge {
            public KevinBarrier Parent;

            public bool Visible;
            public Vector2 A;
            public Vector2 B;
            public Vector2 Min;
            public Vector2 Max;
            public Vector2 Normal;
            public Vector2 Perpendicular;
            public float[] Wave;
            public float Length;

            public Edge(KevinBarrier parent, Vector2 a, Vector2 b) {
                Parent = parent;
                Visible = true;
                A = a;
                B = b;
                Min = new Vector2(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));
                Max = new Vector2(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
                Normal = (b - a).SafeNormalize();
                Perpendicular = -Normal.Perpendicular();
                Length = (a - b).Length();
            }

            public void UpdateWave(float time) {
                if (Wave == null || Wave.Length <= Length) {
                    Wave = new float[(int) Length + 2];
                }
                for (int i = 0; i <= Length; i++) {
                    Wave[i] = GetWaveAt(time, i, Length);
                }
            }

            private float GetWaveAt(float offset, float along, float length) {
                if (along <= 1f || along >= length - 1f) {
                    return 0f;
                }
                if (Parent.Solidify >= 1f) {
                    return 0f;
                }
                float progress = offset + along * 0.25f;
                float progressSine = (float) (Math.Sin(progress) * 2.0 + Math.Sin(progress * 0.25f));
                return (1f + progressSine * Ease.SineInOut(Calc.YoYo(along / length))) * (1f - Parent.Solidify);
            }

            public bool InView(ref Rectangle view) {
                return view.Left < Parent.X + Max.X && view.Right > Parent.X + Min.X && view.Top < Parent.Y + Max.Y && view.Bottom > Parent.Y + Min.Y;
            }
        }

        private List<KevinBarrier> list = new List<KevinBarrier>();
        private List<Edge> edges = new List<Edge>();
        private VirtualMap<bool> tiles;
        private Rectangle levelTileBounds;
        private bool dirty;

        public KevinBarrierRenderer() {
            Tag = (Tags.Global | Tags.TransitionUpdate);
            Depth = 0;
            Add(new CustomBloom(OnRenderBloom));
        }

        public void Track(KevinBarrier block) {
            list.Add(block);
            if (tiles == null) {
                levelTileBounds = (Scene as Level).TileBounds;
                tiles = new VirtualMap<bool>(levelTileBounds.Width, levelTileBounds.Height, emptyValue: false);
            }
            for (int i = (int) block.X / 8; i < block.Right / 8f; i++) {
                for (int j = (int) block.Y / 8; j < block.Bottom / 8f; j++) {
                    tiles[i - levelTileBounds.X, j - levelTileBounds.Y] = true;
                }
            }
            dirty = true;
        }

        public void Untrack(KevinBarrier block) {
            list.Remove(block);
            if (list.Count <= 0) {
                tiles = null;
            } else {
                for (int i = (int) block.X / 8; i < block.Right / 8f; i++) {
                    for (int j = (int) block.Y / 8; j < block.Bottom / 8f; j++) {
                        tiles[i - levelTileBounds.X, j - levelTileBounds.Y] = false;
                    }
                }
            }
            dirty = true;
        }

        public override void Update() {
            if (dirty) {
                RebuildEdges();
            }
            UpdateEdges();
        }

        public void UpdateEdges() {
            Camera camera = (Scene as Level).Camera;
            Rectangle view = new Rectangle((int) camera.Left - 4, (int) camera.Top - 4, (int) (camera.Right - camera.Left) + 8, (int) (camera.Bottom - camera.Top) + 8);
            for (int i = 0; i < edges.Count; i++) {
                if (edges[i].Visible) {
                    if (Scene.OnInterval(0.25f, i * 0.01f) && !edges[i].InView(ref view)) {
                        edges[i].Visible = false;
                    }
                } else if (Scene.OnInterval(0.05f, i * 0.01f) && edges[i].InView(ref view)) {
                    edges[i].Visible = true;
                }
                if (edges[i].Visible && (Scene.OnInterval(0.05f, i * 0.01f) || edges[i].Wave == null)) {
                    edges[i].UpdateWave(Scene.TimeActive * 3f);
                }
            }
        }

        private void RebuildEdges() {
            dirty = false;
            edges.Clear();
            if (list.Count <= 0) {
                return;
            }
            Point[] directions = new Point[4] {
                new Point(0, -1),
                new Point(0, 1),
                new Point(-1, 0),
                new Point(1, 0)
            };
            foreach (KevinBarrier barrier in list) {
                for (int i = (int) barrier.X / 8; i < barrier.Right / 8f; i++) {
                    for (int j = (int) barrier.Y / 8; j < barrier.Bottom / 8f; j++) {
                        for (int k = 0; k < directions.Length; k++) {
                            Point direction = directions[k];
                            Point normal = new Point(-direction.Y, direction.X);
                            if (!Inside(i + direction.X, j + direction.Y) && (!Inside(i - normal.X, j - normal.Y) || Inside(i + direction.X - normal.X, j + direction.Y - normal.Y))) {
                                Point pointHere = new Point(i, j);
                                Point pointPlusNormal = new Point(i + normal.X, j + normal.Y);
                                Vector2 value = new Vector2(4f) + new Vector2(direction.X - normal.X, direction.Y - normal.Y) * 4f;
                                while (Inside(pointPlusNormal.X, pointPlusNormal.Y) && !Inside(pointPlusNormal.X + direction.X, pointPlusNormal.Y + direction.Y)) {
                                    pointPlusNormal.X += normal.X;
                                    pointPlusNormal.Y += normal.Y;
                                }
                                Vector2 a = new Vector2(pointHere.X, pointHere.Y) * 8f + value - barrier.Position;
                                Vector2 b = new Vector2(pointPlusNormal.X, pointPlusNormal.Y) * 8f + value - barrier.Position;
                                edges.Add(new Edge(barrier, a, b));
                            }
                        }
                    }
                }
            }
        }

        private bool Inside(int tx, int ty) {
            return tiles[tx - levelTileBounds.X, ty - levelTileBounds.Y];
        }

        private void OnRenderBloom() {
            foreach (KevinBarrier barrier in list) {
                if (barrier.Visible && !barrier.Invisible) {
                    Draw.Rect(barrier.X, barrier.Y, barrier.Width, barrier.Height, barrier.Color);
                }
            }
            foreach (Edge edge in edges) {
                if (edge.Visible && !edge.Parent.Invisible) {
                    Vector2 position = edge.Parent.Position + edge.A;
                    for (int i = 0; i <= edge.Length; i++) {
                        Vector2 positionPlusNormal = position + edge.Normal * i;
                        Draw.Line(positionPlusNormal, positionPlusNormal + edge.Perpendicular * edge.Wave[i], edge.Parent.Color);
                    }
                }
            }
        }

        public override void Render() {
            if (list.Count <= 0) {
                return;
            }
            foreach (KevinBarrier barrier in list) {
                if (barrier.Visible && !barrier.Invisible) {
                    Draw.Rect(barrier.Collider, barrier.Color);
                }
            }
            if (edges.Count <= 0) {
                return;
            }
            foreach (Edge edge in edges) {
                if (edge.Visible && !edge.Parent.Invisible) {
                    Vector2 position = edge.Parent.Position + edge.A;
                    for (int i = 0; i <= edge.Length; i++) {
                        Vector2 positionPlusNormal = position + edge.Normal * i;
                        Draw.Line(positionPlusNormal, positionPlusNormal + edge.Perpendicular * edge.Wave[i], edge.Parent.Color);
                    }
                }
            }
        }
    }
}
