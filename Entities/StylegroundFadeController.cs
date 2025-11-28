using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
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
        public static void Load() {
            On.Celeste.Backdrop.IsVisible += modBackdropIsVisible;
            On.Celeste.BackdropRenderer.Update += onBackdropRendererUpdate;
            IL.Celeste.BackdropRenderer.Render += modBackdropRendererRender;
        }

        public static void Unload() {
            On.Celeste.Backdrop.IsVisible -= modBackdropIsVisible;
            On.Celeste.BackdropRenderer.Update -= onBackdropRendererUpdate;
            IL.Celeste.BackdropRenderer.Render -= modBackdropRendererRender;
        }

        private static readonly HashSet<StylegroundFadeController> controllerSet = new HashSet<StylegroundFadeController>();

        // dictionary[flag]["Not Flag" setting] = value
        private static readonly Dictionary<string, Dictionary<bool, float>> fades = new Dictionary<string, Dictionary<bool, float>>();
        private static readonly Dictionary<string, Dictionary<bool, float>> fadeInTimes = new Dictionary<string, Dictionary<bool, float>>();
        private static readonly Dictionary<string, Dictionary<bool, float>> fadeOutTimes = new Dictionary<string, Dictionary<bool, float>>();
        private static readonly Dictionary<string, Dictionary<bool, StylegroundFadeController>> controllers = new Dictionary<string, Dictionary<bool, StylegroundFadeController>>();

        // some utility methods for our makeshift "2-dimension dictionaries"
        private static bool tryGetValue<T>(Dictionary<string, Dictionary<bool, T>> dictionary, string flag, bool notFlag, out T value) {
            value = default; // we have to set value no matter what
            return dictionary.TryGetValue(flag, out Dictionary<bool, T> inner) && inner.TryGetValue(notFlag, out value);
        }

        private static void setValue<T>(Dictionary<string, Dictionary<bool, T>> dictionary, string flag, bool notFlag, T value) {
            if (!dictionary.TryGetValue(flag, out Dictionary<bool, T> innerDict)) {
                dictionary[flag] = innerDict = new Dictionary<bool, T>();
            }
            innerDict[notFlag] = value;
            Logger.Log(LogLevel.Verbose, "MaxHelpingHand/StylegroundFadeController", $"Upserted value into dict ({flag}, {notFlag}). Dictionary now has {dictionary.Count} entries.");
        }

        private static void removeValue<T>(Dictionary<string, Dictionary<bool, T>> dictionary, string flag, bool notFlag) {
            dictionary[flag].Remove(notFlag);
            if (dictionary[flag].Count == 0) dictionary.Remove(flag);
            Logger.Log(LogLevel.Verbose, "MaxHelpingHand/StylegroundFadeController", $"Removed value from dict ({flag}, {notFlag}). Dictionary now has {dictionary.Count} entries.");
        }

        private static VirtualRenderTarget tempRenderTarget = null;

        private readonly string[] flags;
        private readonly bool notFlag;
        private readonly float fadeInTime;
        private readonly float fadeOutTime;

        public StylegroundFadeController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flags = data.Attr("flag").Split(',');
            notFlag = data.Bool("notFlag");
            fadeInTime = data.Float("fadeInTime");
            fadeOutTime = data.Float("fadeOutTime");
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            initializeFlag();
        }

        public override void Update() {
            base.Update();

            // Speedrun Tool conveniently backs up fades, fadeInTimes and fadeOutTimes in its savestates, but not controllers.
            // It also skips Awake, so we need to catch up!
            if (!controllerSet.Contains(this)) {
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

        private static void ensureBufferIsCorrect() {
            if (tempRenderTarget == null || tempRenderTarget.Width != MaxHelpingHandModule.BufferWidth || tempRenderTarget.Height != MaxHelpingHandModule.BufferHeight) {
                tempRenderTarget?.Dispose();
                tempRenderTarget = VirtualContent.CreateRenderTarget("max-helping-hand-styleground-fade-controller", MaxHelpingHandModule.BufferWidth, MaxHelpingHandModule.BufferHeight);
            }
        }

        private void initializeFlag() {
            // enable the hooks on backdrop rendering.
            if (controllers.Count == 0) {
                ensureBufferIsCorrect();
            }

            foreach (string flag in flags) {
                // register the current flag settings so that the renderer can pick them.
                bool flagEnabled = SceneAs<Level>().Session.GetFlag(flag);
                if (notFlag) {
                    flagEnabled = !flagEnabled;
                }
                setValue(fades, flag, notFlag, flagEnabled ? 1 : 0);
                setValue(fadeInTimes, flag, notFlag, fadeInTime);
                setValue(fadeOutTimes, flag, notFlag, fadeOutTime);
                setValue(controllers, flag, notFlag, this);
                controllerSet.Add(this);
            }
        }

        private void deregisterFlag() {
            foreach (string flag in flags) {
                // first, make sure there isn't another controller that took the flag over.
                if (tryGetValue(controllers, flag, notFlag, out StylegroundFadeController controller) && controller == this) {
                    // deregister the current flag settings.
                    removeValue(fades, flag, notFlag);
                    removeValue(fadeInTimes, flag, notFlag);
                    removeValue(fadeOutTimes, flag, notFlag);
                    removeValue(controllers, flag, notFlag);
                }
            }

            controllerSet.Remove(this);

            // if there is no flag settings left, disable the hooks on backdrop rendering.
            if (controllers.Count == 0) {
                tempRenderTarget?.Dispose();
                tempRenderTarget = null;
            }
        }

        private static bool modBackdropIsVisible(On.Celeste.Backdrop.orig_IsVisible orig, Backdrop self, Level level) {
            if (controllers.Count == 0) return orig(self, level);

            // force the backdrops that did not fade out yet to be visible, so that the player can see them fade out.
            if (self.OnlyIfFlag != null && tryGetValue(fades, self.OnlyIfFlag, false, out float fade) && fade > 0) {
                return true;
            }
            if (self.OnlyIfNotFlag != null && tryGetValue(fades, self.OnlyIfNotFlag, true, out fade) && fade > 0) {
                return true;
            }

            return orig(self, level);
        }

        private static void onBackdropRendererUpdate(On.Celeste.BackdropRenderer.orig_Update orig, BackdropRenderer self, Scene scene) {
            orig(self, scene);
            if (controllers.Count == 0) return;

            if (scene is Level level) {
                // there are 2 backdrop renderers in that scene (bg and fg), so we are going to be double updating on each frame.
                float delta = Engine.DeltaTime / 2f;

                // update the fades of all the flags that are in the scene.
                foreach (string flag in fades.Keys) {
                    foreach (bool notFlag in fades[flag].Keys) {
                        if (level.Session.GetFlag(flag) != notFlag) {
                            fades[flag][notFlag] = Calc.Approach(fades[flag][notFlag], 1, delta / fadeInTimes[flag][notFlag]);
                        } else {
                            fades[flag][notFlag] = Calc.Approach(fades[flag][notFlag], 0, delta / fadeOutTimes[flag][notFlag]);
                        }
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
                cursor.EmitDelegate<Action<BackdropRenderer, Backdrop>>(renderStylegroundStart);

                cursor.Index++;

                // after the rendering, switch back to the regular render target and render our render target with alpha if necessary.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc, backdropLocal);
                cursor.Emit(OpCodes.Ldloc, blendStateLocal);
                cursor.EmitDelegate<Action<BackdropRenderer, Backdrop, BlendState>>(renderStylegroundEnd);
            }
        }

        private static void renderStylegroundStart(BackdropRenderer self, Backdrop backdrop) {
            if (controllers.Count == 0) return;

            bool hasFlag = backdrop.OnlyIfFlag != null && tryGetValue(fades, backdrop.OnlyIfFlag, false, out float fade) && fade < 1;
            bool hasNotFlag = backdrop.OnlyIfNotFlag != null && tryGetValue(fades, backdrop.OnlyIfNotFlag, true, out fade) && fade < 1;

            if (hasFlag || hasNotFlag) {
                self.EndSpritebatch();

                ensureBufferIsCorrect();
                Engine.Graphics.GraphicsDevice.SetRenderTarget(tempRenderTarget);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);

                if (backdrop.UseSpritebatch) {
                    if (backdrop is Parallax) {
                        self.StartSpritebatchLooping(BlendState.AlphaBlend);
                    } else {
                        self.StartSpritebatch(BlendState.AlphaBlend);
                    }
                }
            }
        }

        private static void renderStylegroundEnd(BackdropRenderer self, Backdrop backdrop, BlendState blendState) {
            if (controllers.Count == 0) return;

            string flag = null;
            bool notFlag = false;
            if (backdrop.OnlyIfFlag != null && tryGetValue(fades, backdrop.OnlyIfFlag, false, out float fade) && fade < 1) {
                flag = backdrop.OnlyIfFlag;
                notFlag = false;
            }
            if (backdrop.OnlyIfNotFlag != null && tryGetValue(fades, backdrop.OnlyIfNotFlag, true, out fade) && fade < 1) {
                flag = backdrop.OnlyIfNotFlag;
                notFlag = true;
            }

            if (flag != null) {
                self.EndSpritebatch();

                Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Level);

                self.StartSpritebatch(blendState);
                Draw.SpriteBatch.Draw(tempRenderTarget, Vector2.Zero, Color.White * fades[flag][notFlag]);
            }
        }
    }
}
