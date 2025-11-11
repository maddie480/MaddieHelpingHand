using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/SecretBerry")]
    [RegisterStrawberry(tracked: false, blocksCollection: true)]
    public class SecretBerry : Strawberry {
        private static ILHook strawberryCollectRoutineHook;
        private static ILHook strawberryCollectRoutineHook2;

        public static void Load() {
            IL.Celeste.Strawberry.OnAnimate += replaceStrawberryStrings;
            IL.Celeste.Strawberry.OnPlayer += replaceStrawberryStrings;
            IL.Celeste.Strawberry.Added += replaceStrawberryStrings;

            MethodInfo collectRoutine = typeof(Strawberry).GetMethod("CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
            strawberryCollectRoutineHook = new ILHook(collectRoutine, replaceStrawberryStrings);
            strawberryCollectRoutineHook2 = new ILHook(collectRoutine, injectMoonBerrySound);

            On.Celeste.Strawberry.CollectRoutine += onStrawberryCollect;
            On.Celeste.Strawberry.Update += onStrawberryUpdate;

            On.Celeste.MapData.Load += onMapDataLoad;

            IL.Celeste.Strawberry.OnAnimate += toggleStrawberryPulse;
        }

        public static void Unload() {
            IL.Celeste.Strawberry.OnAnimate -= replaceStrawberryStrings;
            IL.Celeste.Strawberry.OnPlayer -= replaceStrawberryStrings;

            strawberryCollectRoutineHook?.Dispose();
            strawberryCollectRoutineHook = null;
            strawberryCollectRoutineHook2?.Dispose();
            strawberryCollectRoutineHook2 = null;

            IL.Celeste.Strawberry.Added -= replaceStrawberryStrings;

            On.Celeste.Strawberry.CollectRoutine -= onStrawberryCollect;
            On.Celeste.Strawberry.Update -= onStrawberryUpdate;

            On.Celeste.MapData.Load -= onMapDataLoad;

            IL.Celeste.Strawberry.OnAnimate -= toggleStrawberryPulse;
        }

        private static void replaceStrawberryStrings(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            string[] vanillaPaths = new string[] {
                "event:/game/general/strawberry_pulse", "event:/game/general/strawberry_blue_touch", "event:/game/general/strawberry_touch", "event:/game/general/strawberry_get", "strawberry", "ghostberry"
            };
            Func<SecretBerry, string>[] customPathGetters = new Func<SecretBerry, string>[] {
                b => b.strawberryPulseSound, b => b.strawberryBlueTouchSound, b => b.strawberryTouchSound, b => b.strawberryGetSound, b => b.strawberrySprite, b => b.ghostberrySprite
            };

            // replace all listed sounds.
            for (int i = 0; i < vanillaPaths.Length; i++) {
                Func<SecretBerry, string> customPathGetter = customPathGetters[i];
                while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr(vanillaPaths[i]))) {
                    Logger.Log("MaxHelpingHand/SecretBerry", $"Replacing string \"{vanillaPaths[i]}\" at {cursor.Index} in IL for {il.Method.FullName}");
                    cursor.Emit(OpCodes.Ldarg_0);
                    if (il.Method.Name.Contains("MoveNext")) {
                        // we are hooking the collect coroutine: get the actual "this".
                        cursor.Emit(OpCodes.Ldfld, typeof(Strawberry).GetMethod("CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget().DeclaringType.GetField("<>4__this"));
                    }
                    cursor.EmitDelegate<Func<string, Strawberry, string>>((orig, self) => {
                        if (self is SecretBerry berry) {
                            return customPathGetter(berry);
                        }
                        return orig;
                    });
                }
                cursor.Index = 0;
            }
        }

        private static void injectMoonBerrySound(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Strawberry>("get_Moon"))) {
                Logger.Log("MaxHelpingHand/SecretBerry", $"Injecting moon berry sound at {cursor.Index} in IL for Strawberry.CollectRoutine");

                // get "this" strawberry in order to check if this is a Secret Berry
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(Strawberry).GetMethod("CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetStateMachineTarget().DeclaringType.GetField("<>4__this"));

                cursor.EmitDelegate<Func<bool, Strawberry, bool>>((orig, self) => {
                    if (self is SecretBerry berry && berry.moonBerrySound) {
                        return true; // this will set the "colour" music param to 3, which obviously means "make a moon berry sound"
                    }
                    return orig;
                });
            }
        }

        private static IEnumerator onStrawberryCollect(On.Celeste.Strawberry.orig_CollectRoutine orig, Strawberry self, int collectIndex) {
            Scene scene = self.Scene;

            yield return new SwapImmediately(orig(self, collectIndex));

            if (self is SecretBerry berry) {
                // reskin the strawberry points.
                StrawberryPoints points = scene.Entities.ToAdd.OfType<StrawberryPoints>().FirstOrDefault();
                if (points != null) GFX.SpriteBank.CreateOn(points.Get<Sprite>(), berry.strawberrySprite);
            }
        }

        private static void onStrawberryUpdate(On.Celeste.Strawberry.orig_Update orig, Strawberry self) {
            if (!(self is SecretBerry berry)) {
                // this is a regular berry: it should behave normally.
                orig(self);
                return;
            }

            // back up vanilla particles
            ParticleType origGlow = P_Glow;
            ParticleType origGhostGlow = P_GhostGlow;

            // replace them
            P_Glow = berry.strawberryParticleType;
            P_GhostGlow = berry.strawberryGhostParticleType;

            // run vanilla code (that may emit particles)
            orig(self);

            // place the vanilla particles back
            P_Glow = origGlow;
            P_GhostGlow = origGhostGlow;
        }

        private static void onMapDataLoad(On.Celeste.MapData.orig_Load orig, MapData self) {
            orig(self);

            // if the map data processor detected secret berries that should count towards the total... add them to the total.
            if (MaxHelpingHandMapDataProcessor.DetectedSecretBerries > 0) {
                self.ModeData.TotalStrawberries += MaxHelpingHandMapDataProcessor.DetectedSecretBerries;
                MaxHelpingHandMapDataProcessor.DetectedSecretBerries = 0;
            }
        }

        private static void toggleStrawberryPulse(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Sprite>("get_CurrentAnimationFrame"), instr => instr.MatchLdloc(0))) {
                Logger.Log("MaxHelpingHand/SecretBerry", $"Disabling pulse animation on demand at {cursor.Index} in IL for Strawberry.OnAnimate");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, Strawberry, int>>((orig, self) => {
                    if (self is SecretBerry berry && !berry.pulseEnabled) {
                        // trigger the sound ourselves
                        if (self.Get<Sprite>().CurrentAnimationFrame == orig) {
                            Audio.Play(berry.strawberryPulseSound, berry.Position);
                        }

                        // make the branch triggering the pulse light + displacement effect always false
                        return -1;
                    }
                    return orig;
                });
            }
        }


        private readonly string strawberrySprite;
        private readonly string ghostberrySprite;
        private readonly string strawberryPulseSound;
        private readonly string strawberryBlueTouchSound;
        private readonly string strawberryTouchSound;
        private readonly string strawberryGetSound;
        private readonly bool pulseEnabled;
        private readonly bool spotlightEnabled;
        private readonly bool moonBerrySound;
        private readonly ParticleType strawberryParticleType;
        private readonly ParticleType strawberryGhostParticleType;
        private readonly string visibleIfFlag;
        private StrawberryToggler toggler;

        public SecretBerry(EntityData data, Vector2 offset, EntityID gid) : base(data, offset, gid) {
            strawberrySprite = data.Attr("strawberrySprite");
            ghostberrySprite = data.Attr("ghostberrySprite");
            strawberryPulseSound = data.Attr("strawberryPulseSound");
            strawberryBlueTouchSound = data.Attr("strawberryBlueTouchSound");
            strawberryTouchSound = data.Attr("strawberryTouchSound");
            strawberryGetSound = data.Attr("strawberryGetSound");
            pulseEnabled = data.Bool("pulseEnabled", defaultValue: true);
            spotlightEnabled = data.Bool("spotlightEnabled", defaultValue: true);
            moonBerrySound = data.Bool("moonBerrySound", defaultValue: false);
            visibleIfFlag = data.Attr("visibleIfFlag");

            strawberryParticleType = new ParticleType(P_Glow) {
                Color = Calc.HexToColor(data.Attr("particleColor1")),
                Color2 = Calc.HexToColor(data.Attr("particleColor2")),
            };
            strawberryGhostParticleType = new ParticleType(P_GhostGlow) {
                Color = Calc.HexToColor(data.Attr("ghostParticleColor1")),
                Color2 = Calc.HexToColor(data.Attr("ghostParticleColor2")),
            };
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            if (!string.IsNullOrEmpty(visibleIfFlag) && scene is Level level) {
                scene.Add(toggler = new StrawberryToggler(this, visibleIfFlag, level));
            }

            if (!spotlightEnabled) {
                Remove(Get<VertexLight>());
                Remove(Get<BloomPoint>());
            }
        }

        // this is in a separate entity, because it can freeze the berry entirely...
        // without freezing the process that unfreezes it when the flag is set.
        private class StrawberryToggler : Entity {
            private readonly SecretBerry berry;
            private readonly string flag;
            private readonly Level level;

            public StrawberryToggler(SecretBerry berry, string flag, Level level) {
                this.berry = berry;
                this.flag = flag;
                this.level = level;

                // update right away, to make sure the berry disappears if it needs to
                // without waiting for the first update (which is very noticeable during transitions).
                Update();
            }

            public override void Update() {
                bool isVisible = level.Session.GetFlag(flag);
                berry.Active = berry.Visible = berry.Collidable = isVisible;
                berry.Get<BloomPoint>().Visible = isVisible;
            }
        }

        public override void Update() {
            base.Update();

            if (toggler != null && Follower.Leader != null) {
                // once the berry has been grabbed, it should not be hidden anymore.
                toggler.RemoveSelf();
                toggler = null;
            }
        }
    }
}
