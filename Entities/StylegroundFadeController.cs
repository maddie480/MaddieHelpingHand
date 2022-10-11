using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        // TODO: remove reflection when looping styleground improvements have reached stable
        private static MethodInfo startSpritebatchLooping = typeof(BackdropRenderer).GetMethod("StartSpritebatchLooping");

        private string[] keys;
        private float fadeInTime;
        private float fadeOutTime;

        public StylegroundFadeController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            keys = data.Attr("flag").Split(',');
            fadeInTime = data.Float("fadeInTime");
            fadeOutTime = data.Float("fadeOutTime");

            // settings are unique for a "flag + 'not flag' toggle" pair, we are storing both in a "key".
            // this will allow using the same flag as a "flag" and as a "not flag" and have them controlled by separate controllers.
            string prefix = data.Bool("notFlag") ? "n:" : "f:";
            for (int i = 0; i < keys.Length; i++) {
                keys[i] = prefix + keys[i];
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            initializeFlag();
        }

        public override void Update() {
            base.Update();

            // Speedrun Tool conveniently backs up fades, fadeInTimes and fadeOutTimes in its savestates, but not controllers.
            // It also skips Awake, so we need to catch up!
            if (!controllers.ContainsValue(this)) {
                initializeFlag();
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

        private void initializeFlag() {
            // enable the hooks on backdrop rendering.
            if (controllers.Count == 0) {
                On.Celeste.Backdrop.IsVisible += modBackdropIsVisible;
                On.Celeste.BackdropRenderer.Update += onBackdropRendererUpdate;
                IL.Celeste.BackdropRenderer.Render += modBackdropRendererRender;

                tempRenderTarget = VirtualContent.CreateRenderTarget("max-helping-hand-styleground-fade-controller", 320, 280);
            }

            foreach (string key in keys) {
                // register the current flag settings so that the renderer can pick them.
                bool flagEnabled = SceneAs<Level>().Session.GetFlag(key.Substring(2));
                if (key.StartsWith("n:")) {
                    flagEnabled = !flagEnabled;
                }

                fades[key] = flagEnabled ? 1 : 0;
                fadeInTimes[key] = fadeInTime;
                fadeOutTimes[key] = fadeOutTime;
                controllers[key] = this;
            }
        }

        private void deregisterFlag() {
            foreach (string key in keys) {
                // first, make sure there isn't another controller that took the flag over.
                if (controllers[key] == this) {
                    // deregister the current flag settings.
                    fades.Remove(key);
                    fadeInTimes.Remove(key);
                    fadeOutTimes.Remove(key);
                    controllers.Remove(key);
                }
            }

            // if there is no flag settings left, disable the hooks on backdrop rendering.
            if (controllers.Count == 0) {
                On.Celeste.Backdrop.IsVisible -= modBackdropIsVisible;
                On.Celeste.BackdropRenderer.Update -= onBackdropRendererUpdate;
                IL.Celeste.BackdropRenderer.Render -= modBackdropRendererRender;

                tempRenderTarget?.Dispose();
                tempRenderTarget = null;
            }
        }

        private static bool modBackdropIsVisible(On.Celeste.Backdrop.orig_IsVisible orig, Backdrop self, Level level) {
            // force the backdrops that did not fade out yet to be visible, so that the player can see them fade out.
            if (self.OnlyIfFlag != null && fades.ContainsKey("f:" + self.OnlyIfFlag) && fades["f:" + self.OnlyIfFlag] > 0) {
                return true;
            }
            if (self.OnlyIfNotFlag != null && fades.ContainsKey("n:" + self.OnlyIfNotFlag) && fades["n:" + self.OnlyIfNotFlag] > 0) {
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
                foreach (string key in fades.Keys.ToList()) {
                    if (level.Session.GetFlag(key.Substring(2)) == key.StartsWith("f:")) {
                        fades[key] = Calc.Approach(fades[key], 1, delta / fadeInTimes[key]);
                    } else {
                        fades[key] = Calc.Approach(fades[key], 0, delta / fadeOutTimes[key]);
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
                cursor.EmitDelegate<Action<BackdropRenderer, Backdrop>>((self, backdrop) => {
                    bool hasFlag = backdrop.OnlyIfFlag != null && fades.ContainsKey("f:" + backdrop.OnlyIfFlag) && fades["f:" + backdrop.OnlyIfFlag] < 1;
                    bool hasNotFlag = backdrop.OnlyIfNotFlag != null && fades.ContainsKey("n:" + backdrop.OnlyIfNotFlag) && fades["n:" + backdrop.OnlyIfNotFlag] < 1;

                    if (hasFlag || hasNotFlag) {
                        self.EndSpritebatch();

                        Engine.Graphics.GraphicsDevice.SetRenderTarget(tempRenderTarget);
                        Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

                        if (backdrop.UseSpritebatch) {
                            if (startSpritebatchLooping != null && backdrop is Parallax) {
                                startSpritebatchLooping.Invoke(self, new object[] { BlendState.AlphaBlend });
                            } else {
                                self.StartSpritebatch(BlendState.AlphaBlend);
                            }
                        }
                    }
                });

                cursor.Index++;

                // after the rendering, switch back to the regular render target and render our render target with alpha if necessary.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc, backdropLocal);
                cursor.Emit(OpCodes.Ldloc, blendStateLocal);
                cursor.EmitDelegate<Action<BackdropRenderer, Backdrop, BlendState>>((self, backdrop, blendState) => {
                    string key = null;
                    if (backdrop.OnlyIfFlag != null && fades.ContainsKey("f:" + backdrop.OnlyIfFlag) && fades["f:" + backdrop.OnlyIfFlag] < 1) {
                        key = "f:" + backdrop.OnlyIfFlag;
                    }
                    if (backdrop.OnlyIfNotFlag != null && fades.ContainsKey("n:" + backdrop.OnlyIfNotFlag) && fades["n:" + backdrop.OnlyIfNotFlag] < 1) {
                        key = "n:" + backdrop.OnlyIfNotFlag;
                    }

                    if (key != null) {
                        self.EndSpritebatch();

                        Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);

                        self.StartSpritebatch(blendState);
                        Draw.SpriteBatch.Draw(tempRenderTarget, Vector2.Zero, Color.White * fades[key]);
                    }
                });

            }
        }
    }
}
