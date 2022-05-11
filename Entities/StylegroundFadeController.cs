using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    /// <summary>
    /// A controller that makes arbitrary stylegrounds fade in and out.
    /// </summary>
    [CustomEntity("MaxHelpingHand/StylegroundFadeController")]
    [Tracked]
    public class StylegroundFadeController : Entity {
        private static Dictionary<string, float> fades = new Dictionary<string, float>();
        private static Dictionary<string, float> fadeInTimes = new Dictionary<string, float>();
        private static Dictionary<string, float> fadeOutTimes = new Dictionary<string, float>();
        private static Dictionary<string, StylegroundFadeController> controllers = new Dictionary<string, StylegroundFadeController>();

        private static VirtualRenderTarget tempRenderTarget = null;

        private string[] flags;
        private float fadeInTime;
        private float fadeOutTime;

        public StylegroundFadeController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flags = data.Attr("flag").Split(',');
            fadeInTime = data.Float("fadeInTime");
            fadeOutTime = data.Float("fadeOutTime");
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            // enable the hooks on backdrop rendering.
            if (fades.Count == 0) {
                On.Celeste.Backdrop.IsVisible += modBackdropIsVisible;
                On.Celeste.BackdropRenderer.Update += onBackdropRendererUpdate;
                IL.Celeste.BackdropRenderer.Render += modBackdropRendererRender;

                tempRenderTarget = VirtualContent.CreateRenderTarget("max-helping-hand-styleground-fade-controller", 320, 280);
            }

            foreach (string flag in flags) {
                // register the current flag settings so that the renderer can pick them.
                fades[flag] = SceneAs<Level>().Session.GetFlag(flag) ? 1 : 0;
                fadeInTimes[flag] = fadeInTime;
                fadeOutTimes[flag] = fadeOutTime;
                controllers[flag] = this;
            }
        }

        public override void Removed(Scene scene) {
            base.Removed(scene);
            deregisterFlag();
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);
            deregisterFlag();
        }

        private void deregisterFlag() {
            foreach (string flag in flags) {
                // first, make sure there isn't another controller that took the flag over.
                if (controllers[flag] == this) {
                    // deregister the current flag settings.
                    fades.Remove(flag);
                    fadeInTimes.Remove(flag);
                    fadeOutTimes.Remove(flag);
                    controllers.Remove(flag);
                }
            }

            // if there is no flag settings left, disable the hooks on backdrop rendering.
            if (fades.Count == 0) {
                On.Celeste.Backdrop.IsVisible -= modBackdropIsVisible;
                On.Celeste.BackdropRenderer.Update -= onBackdropRendererUpdate;
                IL.Celeste.BackdropRenderer.Render -= modBackdropRendererRender;

                tempRenderTarget?.Dispose();
                tempRenderTarget = null;
            }
        }

        private static bool modBackdropIsVisible(On.Celeste.Backdrop.orig_IsVisible orig, Backdrop self, Level level) {
            // force the backdrops that did not fade out yet to be visible, so that the player can see them fade out.
            if (self.OnlyIfFlag != null && fades.ContainsKey(self.OnlyIfFlag) && fades[self.OnlyIfFlag] > 0) {
                return true;
            }

            return orig(self, level);
        }

        private static void onBackdropRendererUpdate(On.Celeste.BackdropRenderer.orig_Update orig, BackdropRenderer self, Scene scene) {
            orig(self, scene);

            if (scene is Level level) {
                // there are 2 backdrop renderers in that scene (bg and fg), so we are going to be double updating on each frame.
                float delta = Engine.DeltaTime / 2f;

                // update the fades of all the flags that are in the scene.
                foreach (string flag in fades.Keys.ToList()) {
                    if (level.Session.GetFlag(flag)) {
                        fades[flag] = Calc.Approach(fades[flag], 1, delta / fadeInTimes[flag]);
                    } else {
                        fades[flag] = Calc.Approach(fades[flag], 0, delta / fadeOutTimes[flag]);
                    }
                }
            }
        }

        private static void modBackdropRendererRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            VariableDefinition blendStateLocal = null;
            VariableDefinition backdropLocal = null;

            foreach (VariableDefinition variable in il.Body.Variables) {
                if (variable.VariableType.FullName == "Microsoft.Xna.Framework.Graphics.BlendState") {
                    blendStateLocal = variable;
                }
                if (variable.VariableType.FullName == "Celeste.Backdrop") {
                    backdropLocal = variable;
                }
            }

            Logger.Log("MaxHelpingHand/StylegroundFadeController", $"The blend state is local variable {blendStateLocal.Index}, the backdrop is local variable {backdropLocal.Index}");


            if (cursor.TryGotoNext(instr => instr.MatchCallvirt<Backdrop>("Render"))) {
                Logger.Log("MaxHelpingHand/StylegroundFadeController", $"Modding backdrop rendering at {cursor.Index} in IL for BackdropRenderer.Render");

                // before the rendering, switch to a dedicated render target if required.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc, backdropLocal);
                cursor.Emit(OpCodes.Ldloc, blendStateLocal);
                cursor.EmitDelegate<Action<BackdropRenderer, Backdrop, BlendState>>((self, backdrop, blendState) => {
                    if (backdrop.OnlyIfFlag != null && fades.ContainsKey(backdrop.OnlyIfFlag) && fades[backdrop.OnlyIfFlag] < 1) {
                        self.EndSpritebatch();

                        Engine.Graphics.GraphicsDevice.SetRenderTarget(tempRenderTarget);
                        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

                        if (backdrop.UseSpritebatch) {
                            self.StartSpritebatch(blendState);
                        }
                    }
                });

                cursor.Index++;

                // after the rendering, switch back to the regular render target and render our render target with alpha if necessary.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc, backdropLocal);
                cursor.Emit(OpCodes.Ldloc, blendStateLocal);
                cursor.EmitDelegate<Action<BackdropRenderer, Backdrop, BlendState>>((self, backdrop, blendState) => {
                    if (backdrop.OnlyIfFlag != null && fades.ContainsKey(backdrop.OnlyIfFlag) && fades[backdrop.OnlyIfFlag] < 1) {
                        self.EndSpritebatch();

                        Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);

                        self.StartSpritebatch(blendState);
                        Draw.SpriteBatch.Draw(tempRenderTarget, Vector2.Zero, Color.White * fades[backdrop.OnlyIfFlag]);
                    }
                });

            }
        }
    }
}
