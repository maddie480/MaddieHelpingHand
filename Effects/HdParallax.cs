using Celeste.Mod.MaxHelpingHand.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod;
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

                Logger.Log("MaxHelpingHand/HdParallax", $"Inserting HD BG parallax rendering at {cursor.Index} in IL for Level.Render");
                cursor.EmitDelegate<Action>(renderHdParallaxesBg);

                if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<SpriteBatch>("End"))) {
                    Logger.Log("MaxHelpingHand/HdParallax", $"Inserting HD FG parallax rendering at {cursor.Index} in IL for Level.Render");
                    cursor.EmitDelegate<Action>(renderHdParallaxesFg);
                }
            }
        }

        private static void renderHdParallaxesBg() => renderHdParallaxes(fg: false);
        private static void renderHdParallaxesFg() => renderHdParallaxes(fg: true);

        private static void renderHdParallaxes(bool fg) {
            if (Engine.Scene is Level level) {
                foreach (Backdrop backdrop in (fg ? level.Foreground.Backdrops : level.Background.Backdrops)) {
                    if (backdrop is HdParallax or AnimatedHdParallax) {
                        level.BackgroundColor = Color.Transparent;
                        renderForReal((Parallax) backdrop, level);
                    }
                }
            }
        }

        // (Animated)HdParallax.Render is a no-op that cancels out regular rendering,
        // so we need renderForReal to forcibly call the base Render method to actually render.
        [MonoModLinkTo("Celeste.Parallax", "System.Void Render(Monocle.Scene)")]
        [MonoModForceCall]
        private static void base_Render(Parallax self, Scene scene) {
            throw new NotImplementedException("WTF? MonoModLinkTo is supposed to have relinked calls to this method!");
        }

        static void renderForReal(Parallax parallax, Scene scene) {
            if (!parallax.Visible && !ParallaxFadeOutController.IsParallaxVisible(parallax)) {
                return;
            }

            Matrix matrix = Engine.ScreenMatrix;
            if (SaveData.Instance.Assists.MirrorMode) {
                matrix *= Matrix.CreateTranslation(-Engine.Viewport.Width, 0f, 0f);
                matrix *= Matrix.CreateScale(-1f, 1f, 1f);
            }

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, parallax.BlendState, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, ColorGrade.Effect, matrix);
            base_Render(parallax, scene);
            Draw.SpriteBatch.End();
        }

        private static void onParallaxRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            float[] lookingFor = { 90f, 160f, 320f, 180f };
            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Ldc_R4 && lookingFor.Contains((float) instr.Operand))) {
                Logger.Log("MaxHelpingHand/HdParallax", $"Replacing parallax resolution at {cursor.Index} in IL for Parallax.Render");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Parallax, float>>(makeParallaxHD);
            }
        }

        private static float makeParallaxHD(float orig, Parallax self) {
            if (self is HdParallax or AnimatedHdParallax) {
                return orig * 6; // 1920x1080 is 6 times 320x180.
            }
            return orig;
        }
    }
}
