using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // Strongly based on vanilla LavaRect, except their upper side is left or right, and bubbles go sideways.
    public class SidewaysLavaRect : Component {
        public enum OnlyModes {
            OnlyLeft,
            OnlyRight
        }

        private struct Bubble {
            public Vector2 Position;
            public float Speed;
            public float Alpha;
            public bool Expired; // needs to be reshuffled on respawn
        }

        private struct SurfaceBubble {
            public float Y;
            public float Frame;
            public byte Animation;
        }

        public Vector2 Position;

        public float Fade = 16f;
        public float Spikey = 0f;

        public OnlyModes OnlyMode;

        public float SmallWaveAmplitude = 1f;
        public float BigWaveAmplitude = 4f;
        public float CurveAmplitude = 7f;
        public float UpdateMultiplier = 1f;

        public Color SurfaceColor = Color.White;
        public Color EdgeColor = Color.LightGray;
        public Color CenterColor = Color.DarkGray;

        private float timer = Calc.Random.NextFloat(100f);

        private VertexPositionColor[] verts;
        private int vertCount;

        private bool dirty;

        private Bubble[] bubbles;
        private SurfaceBubble[] surfaceBubbles;
        private int surfaceBubbleIndex;
        private List<List<MTexture>> surfaceBubbleAnimations;

        public int SurfaceStep {
            get;
            private set;
        }

        public float Width {
            get;
            private set;
        }

        public float Height {
            get;
            private set;
        }

        public SidewaysLavaRect(float width, float height, int step, OnlyModes onlyMode)
            : base(active: true, visible: true) {

            OnlyMode = onlyMode;

            Resize(width, height, step);

            surfaceBubbleAnimations = new List<List<MTexture>>();
            surfaceBubbleAnimations.Add(GFX.Game.GetAtlasSubtextures(OnlyMode == OnlyModes.OnlyLeft ?
                "danger/MaxHelpingHand/sidewayslava/bubble_right_a" : "danger/MaxHelpingHand/sidewayslava/bubble_left_a"));
        }

        private void shuffleBubble(int i) {
            bubbles[i].Position = new Vector2(1f + Calc.Random.NextFloat(Width - 2f), Calc.Random.NextFloat(Height));
            bubbles[i].Speed = Calc.Random.Range(4, 12);
            bubbles[i].Alpha = Calc.Random.Range(0.4f, 0.8f);
        }

        public void Resize(float width, float height, int step) {
            float oldWidth = Width;

            Width = width;
            Height = height;
            SurfaceStep = step;
            dirty = true;

            int surroundingSize = (int) (width / SurfaceStep * 2f + height / SurfaceStep * 2f + 4f);
            verts = new VertexPositionColor[surroundingSize * 3 * 6 + 6];

            Bubble[] oldBubbles = bubbles ?? new Bubble[0];
            bubbles = new Bubble[(int) (width * height * 0.005f)];

            // copy old bubbles over, but mark them as expired.
            for (int i = 0; i < oldBubbles.Length && i < bubbles.Length; i++) {
                bubbles[i] = oldBubbles[i];
                bubbles[i].Expired = true;
                if (OnlyMode == OnlyModes.OnlyLeft) {
                    // rescaling looks less weird if we keep the same distance from the right side
                    bubbles[i].Position.X += Width - oldWidth;
                }
            }
            // spread new bubbles in the LavaRect.
            for (int i = oldBubbles.Length; i < bubbles.Length; i++) {
                shuffleBubble(i);
            }

            // copy old surface bubbles over, and initialize new ones.
            SurfaceBubble[] oldSurfaceBubbles = surfaceBubbles ?? new SurfaceBubble[0];
            surfaceBubbles = new SurfaceBubble[(int) Math.Max(4f, bubbles.Length * 0.25f)];
            for (int i = 0; i < oldSurfaceBubbles.Length && i < surfaceBubbles.Length; i++) {
                surfaceBubbles[i] = oldSurfaceBubbles[i];
            }
            for (int j = oldSurfaceBubbles.Length; j < surfaceBubbles.Length; j++) {
                surfaceBubbles[j].Y = -1f;
            }
            surfaceBubbleIndex %= surfaceBubbles.Length;
        }

        public override void Update() {
            timer += UpdateMultiplier * Engine.DeltaTime;
            if (UpdateMultiplier != 0f) {
                dirty = true;
            }

            // make bubbles bubble "up" (actually right).
            for (int i = 0; i < bubbles.Length; i++) {
                bool limitReached = false;

                if (OnlyMode == OnlyModes.OnlyLeft) {
                    bubbles[i].Position.X += UpdateMultiplier * bubbles[i].Speed * Engine.DeltaTime;
                    if (bubbles[i].Position.X > Width - 2f + Wave((int) (bubbles[i].Position.Y / SurfaceStep), Height)) {
                        // bubble reached the surface. reset it
                        bubbles[i].Position.X = 1f;
                        limitReached = true;
                    }
                } else {
                    bubbles[i].Position.X -= UpdateMultiplier * bubbles[i].Speed * Engine.DeltaTime;
                    if (bubbles[i].Position.X < 2f - Wave((int) (bubbles[i].Position.Y / SurfaceStep), Height)) {
                        // bubble reached the surface. reset it
                        bubbles[i].Position.X = Width - 1f;
                        limitReached = true;
                    }
                }

                if (limitReached) {
                    if (bubbles[i].Expired) {
                        // lava was resized since the bubble spawn: we need to reshuffle it!
                        shuffleBubble(i);
                        bubbles[i].Expired = false;
                    }

                    if (Calc.Random.Chance(0.75f)) {
                        // we want the bubble to explode at the surface.
                        surfaceBubbles[surfaceBubbleIndex].Y = bubbles[i].Position.Y;
                        surfaceBubbles[surfaceBubbleIndex].Frame = 0f;
                        surfaceBubbles[surfaceBubbleIndex].Animation = (byte) Calc.Random.Next(surfaceBubbleAnimations.Count);
                        surfaceBubbleIndex = (surfaceBubbleIndex + 1) % surfaceBubbles.Length;
                    }
                }
            }

            // make surface bubbles animations advance.
            for (int j = 0; j < surfaceBubbles.Length; j++) {
                if (surfaceBubbles[j].Y >= 0f) {
                    surfaceBubbles[j].Frame += Engine.DeltaTime * 6f;
                    if (surfaceBubbles[j].Frame >= surfaceBubbleAnimations[surfaceBubbles[j].Animation].Count) {
                        // animation is over: clean up!
                        surfaceBubbles[j].Y = -1f;
                    }
                }
            }

            base.Update();
        }

        private float Sin(float value) {
            return (1f + (float) Math.Sin(value)) / 2f;
        }

        private float Wave(int step, float length) {
            int stepOffset = step * SurfaceStep;
            float waveOffset = Sin(stepOffset * 0.25f + timer * 4f) * SmallWaveAmplitude;
            waveOffset += Sin(stepOffset * 0.05f + timer * 0.5f) * BigWaveAmplitude;
            if (step % 2 == 0) {
                waveOffset += Spikey;
            }
            waveOffset += (1f - Calc.YoYo(stepOffset / length)) * CurveAmplitude;
            return waveOffset;
        }

        private void Quad(ref int vert, Vector2 va, Vector2 vb, Vector2 vc, Vector2 vd, Color color) {
            Quad(ref vert, va, color, vb, color, vc, color, vd, color);
        }

        private void Quad(ref int vert, Vector2 va, Color ca, Vector2 vb, Color cb, Vector2 vc, Color cc, Vector2 vd, Color cd) {
            verts[vert].Position.X = va.X;
            verts[vert].Position.Y = va.Y;
            verts[vert++].Color = ca;
            verts[vert].Position.X = vb.X;
            verts[vert].Position.Y = vb.Y;
            verts[vert++].Color = cb;
            verts[vert].Position.X = vc.X;
            verts[vert].Position.Y = vc.Y;
            verts[vert++].Color = cc;
            verts[vert].Position.X = va.X;
            verts[vert].Position.Y = va.Y;
            verts[vert++].Color = ca;
            verts[vert].Position.X = vc.X;
            verts[vert].Position.Y = vc.Y;
            verts[vert++].Color = cc;
            verts[vert].Position.X = vd.X;
            verts[vert].Position.Y = vd.Y;
            verts[vert++].Color = cd;
        }

        private void Edge(ref int vert, Vector2 a, Vector2 b, float fade) {
            float edgeSize = (a - b).Length();
            float stepCount = edgeSize / SurfaceStep;
            Vector2 direction = (b - a).SafeNormalize();
            Vector2 perpendicular = direction.Perpendicular();
            for (int i = 1; i <= stepCount; i++) {
                // values for the previous position
                Vector2 positionBefore = Vector2.Lerp(a, b, (i - 1) / stepCount);
                float waveOffsetBefore = Wave(i - 1, edgeSize);
                Vector2 wavePositionBefore = positionBefore - perpendicular * waveOffsetBefore;

                // values for the current position
                Vector2 position = Vector2.Lerp(a, b, i / stepCount);
                float waveOffset = Wave(i, edgeSize);
                Vector2 wavePosition = position - perpendicular * waveOffset;

                Quad(ref vert, wavePositionBefore + perpendicular, EdgeColor,
                    wavePosition + perpendicular, EdgeColor,
                    position + perpendicular * (fade - waveOffset), CenterColor,
                    positionBefore + perpendicular * (fade - waveOffsetBefore), CenterColor);

                Quad(ref vert, positionBefore + perpendicular * (fade - waveOffsetBefore),
                    position + perpendicular * (fade - waveOffset),
                    position + perpendicular * fade,
                    positionBefore + perpendicular * fade,
                    CenterColor);

                Quad(ref vert, wavePositionBefore,
                    wavePosition,
                    wavePosition + perpendicular,
                    wavePositionBefore + perpendicular * 1f,
                    SurfaceColor);
            }
        }

        public override void Render() {
            GameplayRenderer.End();

            // render the edges of the lava rect.
            if (dirty) {
                Vector2 zero = Vector2.Zero;

                Vector2 topLeft = zero;
                Vector2 topRight = new Vector2(zero.X + Width, zero.Y);
                Vector2 bottomLeft = new Vector2(zero.X, zero.Y + Height);
                Vector2 bottomRight = zero + new Vector2(Width, Height);

                Vector2 fadeOffset = new Vector2(Math.Min(Fade, Width / 2f), Math.Min(Fade, Height / 2f));
                vertCount = 0;
                if (OnlyMode == OnlyModes.OnlyLeft) {
                    Edge(ref vertCount, topRight, bottomRight, fadeOffset.X);
                    Quad(ref vertCount, topLeft, topRight - new Vector2(fadeOffset.X, 0f), bottomRight - new Vector2(fadeOffset.X, 0f), bottomLeft, CenterColor);
                } else if (OnlyMode == OnlyModes.OnlyRight) {
                    Edge(ref vertCount, bottomLeft, topLeft, fadeOffset.X);
                    Quad(ref vertCount, topLeft + new Vector2(fadeOffset.X, 0f), topRight, bottomRight, bottomLeft + new Vector2(fadeOffset.X, 0f), CenterColor);
                }

                dirty = false;
            }

            // render the vertices
            Camera camera = (Scene as Level).Camera;
            GFX.DrawVertices(Matrix.CreateTranslation(new Vector3(Entity.Position + Position, 0f)) * camera.Matrix, verts, vertCount);

            GameplayRenderer.Begin();

            Vector2 rectPosition = Entity.Position + Position;

            // render bubbles
            MTexture bubbleTexture = GFX.Game["particles/bubble"];
            for (int i = 0; i < bubbles.Length; i++) {
                bubbleTexture.DrawCentered(rectPosition + bubbles[i].Position, SurfaceColor * bubbles[i].Alpha);
            }

            // render surface bubbles
            for (int j = 0; j < surfaceBubbles.Length; j++) {
                if (surfaceBubbles[j].Y >= 0f) {
                    MTexture surfaceBubbleTexture = surfaceBubbleAnimations[surfaceBubbles[j].Animation][(int) surfaceBubbles[j].Frame];
                    int bubbleOffset = (int) (surfaceBubbles[j].Y / SurfaceStep);

                    float x;
                    if (OnlyMode == OnlyModes.OnlyLeft)
                        x = Width + 7f + Wave(bubbleOffset, Height);
                    else
                        x = 1f - Wave(bubbleOffset, Height);

                    surfaceBubbleTexture.DrawJustified(rectPosition +
                        new Vector2(x, OnlyMode == OnlyModes.OnlyRight ? Height - bubbleOffset * SurfaceStep : bubbleOffset * SurfaceStep),
                        new Vector2(1f, 0.5f), SurfaceColor);
                }
            }
        }
    }
}
