using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Diagnostics;
using System.IO;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public class CustomWipe : ScreenWipe {
        // the current wipe animation (lazily loaded and 1 at a time)
        private static string currentWipeAnimation = null;
        private static VertexPositionColor[][] currentWipeIn;
        private static VertexPositionColor[][] currentWipeOut;

        public static void Load() {
            On.Celeste.Mod.Meta.MapMeta.ApplyTo += onParseScreenWipe;
        }

        public static void Unload() {
            On.Celeste.Mod.Meta.MapMeta.ApplyTo -= onParseScreenWipe;
        }

        private static void onParseScreenWipe(On.Celeste.Mod.Meta.MapMeta.orig_ApplyTo orig, Meta.MapMeta self, AreaData area) {
            orig(self, area);

            if (!string.IsNullOrEmpty(self.Wipe) && self.Wipe.StartsWith("MaxHelpingHand/CustomWipe:")) {
                // this is a custom wipe!
                string wipeName = self.Wipe.Substring("MaxHelpingHand/CustomWipe:".Length);

                area.Wipe = (scene, wipeIn, onComplete) => {
                    if (wipeName != currentWipeAnimation) {
                        // lazily load the wipe. (we can afford this because this generally lasts less than 100 ms)
                        Stopwatch stopwatch = Stopwatch.StartNew();

                        if (Everest.Content.Map.ContainsKey("MaxHelpingHandWipes/" + wipeName + "/spawn-wipe.bin")) {
                            currentWipeIn = parseWipe("MaxHelpingHandWipes/" + wipeName + "/spawn-wipe.bin");
                        } else {
                            currentWipeIn = parseWipe("MaxHelpingHandWipes/" + wipeName + "/wipe-in.bin");
                        }

                        if (Everest.Content.Map.ContainsKey("MaxHelpingHandWipes/" + wipeName + "/death-wipe.bin")) {
                            currentWipeOut = parseWipe("MaxHelpingHandWipes/" + wipeName + "/death-wipe.bin");
                        } else {
                            currentWipeOut = parseWipe("MaxHelpingHandWipes/" + wipeName + "/wipe-out.bin");
                        }

                        stopwatch.Stop();

                        currentWipeAnimation = wipeName;
                        Logger.Log(LogLevel.Debug, "MaxHelpingHand/CustomWipe", $"Loaded custom screen wipe MaxHelpingHandWipes/{wipeName} in {stopwatch.ElapsedMilliseconds} ms");
                    }

                    // build the custom wipe.
                    new CustomWipe(scene, wipeIn, onComplete, wipeIn ? currentWipeIn : currentWipeOut);
                };

                // let's make sure the wipe is in map metadata because this can get weird.
                if (area.GetMeta() != null) {
                    area.GetMeta().Wipe = self.Wipe;
                }
            }
        }

        private static VertexPositionColor[][] parseWipe(string wipeName) {
            // the binary format is: [frame count], for each frame { [coordinate count], [coordinate list] }
            // all numbers are encoded on 2 bytes, except [coordinate count] because it may exceed 65535.
            using (BinaryReader reader = new BinaryReader(Everest.Content.Map[wipeName].Stream)) {
                // read frame count
                int frameCount = reader.ReadUInt16();
                VertexPositionColor[][] frames = new VertexPositionColor[frameCount][];

                for (int i = 0; i < frameCount; i++) {
                    // read coordinate count, and divide by 2 because each position is 2 coordinates: [x, y] so 2 coordinates => 1 VertexPositionColor
                    int coordCount = (int) (reader.ReadUInt32() / 2);

                    // read all coordinates and convert to VertexPositionColor
                    VertexPositionColor[] coords = new VertexPositionColor[coordCount];
                    for (int j = 0; j < coordCount; j++) {
                        int x = reader.ReadUInt16();
                        int y = reader.ReadUInt16();

                        coords[j] = new VertexPositionColor(new Vector3(x, y, 0f), default);
                    }

                    // store the result in the frames array
                    frames[i] = coords;
                }
                return frames;
            }
        }

        private readonly VertexPositionColor[][] frames;

        public CustomWipe(Scene scene, bool wipeIn, Action onComplete, VertexPositionColor[][] frames) : base(scene, wipeIn, onComplete) {
            this.frames = frames;

            // change the frames' colors to make them match the screen color.
            for (int i = 0; i < frames.Length; i++) {
                VertexPositionColor[] frame = frames[i];
                for (int j = 0; j < frame.Length; j++) {
                    frame[j].Color = WipeColor;
                }
            };
        }

        public override void Render(Scene scene) {
            // draw the triangles matching the current frame.
            VertexPositionColor[] frameToDisplay = frames[Percent >= 1 ? frames.Length - 1 : (int) (frames.Length * Percent)];

            if (frameToDisplay.Length > 0) {
                DrawPrimitives(frameToDisplay);
            }
        }
    }
}
