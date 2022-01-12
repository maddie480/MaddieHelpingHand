using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Effects {
    public class HdParallax : Parallax {
        public static void Load() {
            IL.Celeste.Level.Render += onLevelRender;
            IL.Celeste.Parallax.Render += onParallaxRender;
        }

        public static void Unload() {
            IL.Celeste.Level.Render -= onLevelRender;
            IL.Celeste.Parallax.Render -= onParallaxRender;
        }

        public HdParallax(MTexture texture) : base(texture) { }

        public override void Render(Scene scene) {
            // don't render the usual way!
        }

        private static void onLevelRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(instr => instr.MatchLdnull(), instr => instr.MatchCallvirt<GraphicsDevice>("SetRenderTarget"))
                && cursor.TryGotoNext(instr => instr.MatchCallvirt<SpriteBatch>("Begin"))) {

                Logger.Log("MaxHelpingHand/HdParallax", $"Inserting HD parallax rendering at {cursor.Index} in IL for Level.Render");
                cursor.EmitDelegate<Action>(renderHdParallaxes);
            }
        }

        private static void renderHdParallaxes() {
            if (Engine.Scene is Level level) {
                foreach (Backdrop backdrop in level.Background.Backdrops) {
                    if (backdrop is HdParallax hdParallax) {
                        level.BackgroundColor = Color.Transparent;
                        hdParallax.renderForReal(level);
                    }
                }
            }
        }

        private void renderForReal(Scene scene) {
            Matrix matrix = Engine.ScreenMatrix;
            if (SaveData.Instance.Assists.MirrorMode) {
                matrix *= Matrix.CreateTranslation(-Engine.Viewport.Width, 0f, 0f);
                matrix *= Matrix.CreateScale(-1f, 1f, 1f);
            }

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, ColorGrade.Effect, matrix);
            base.Render(scene);
            Draw.SpriteBatch.End();
        }

        private static void onParallaxRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            float[] lookingFor = { 90f, 160f, 320f, 180f };
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && lookingFor.Contains((float) instr.Operand))) {
                Logger.Log("MaxHelpingHand/HdParallax", $"Replacing parallax resolution at {cursor.Index} in IL for Parallax.Render");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Parallax, float>>((orig, self) => {
                    if (self is HdParallax) {
                        return orig * 6; // 1920x1080 is 6 times 320x180.
                    }
                    return orig;
                });
            }
        }
    }
}
