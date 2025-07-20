using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Celeste.Mod.EeveeHelper.Entities;
using Celeste.Mod.ExCameraDynamics;
using Celeste.Mod.MaxHelpingHand.Module;
using FrostHelper;
using Mono.Cecil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomSeekerBarrier")]
    [TrackedAs(typeof(SeekerBarrier))]
    public class CustomSeekerBarrier : SeekerBarrier {
        public static void Load() {
            IL.Celeste.Seeker.Update += onSeekerUpdate;
            IL.Celeste.Glider.Update += onJellyUpdate;
            IL.Celeste.BloomRenderer.Apply += BloomRendererOnApply;
        }

        public static void LoadMods() {
            using (new DetourContext { After = { "*" } }) { 
                if (eeveeHoldableContainerUpdateHook == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "EeveeHelper", Version = new Version(1, 12, 2) })) {
                    Type holdableContainerType = Everest.Modules.First(mod => mod.GetType().ToString() == "Celeste.Mod.EeveeHelper.EeveeHelperModule")
                        .GetType().Assembly.GetType("Celeste.Mod.EeveeHelper.Entities.HoldableContainer");

                    eeveeHoldableContainerUpdateHook = new ILHook(
                        holdableContainerType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                        onHoldableContainerUpdate
                    );
                }
            }
        }
        public static void Unload() {
            IL.Celeste.Seeker.Update -= onSeekerUpdate;
            IL.Celeste.Glider.Update -= onJellyUpdate;
            IL.Celeste.BloomRenderer.Apply -= BloomRendererOnApply;
            
            eeveeHoldableContainerUpdateHook?.Dispose();
            eeveeHoldableContainerUpdateHook = null;
        }
 
        private static void BloomRendererOnApply(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(
                    ins => ins.MatchLdloca(9),
                    ins => ins.MatchCall(out MethodReference _),
                    ins => ins.MatchCallvirt(out MethodReference _),
                    ins => ins.MatchCall(out MethodReference _),
                    ins => ins.MatchCall(out MethodReference _)
                ))
            {

                MethodInfo getCurrentMethod = typeof(List<Entity>.Enumerator).GetProperty("Current").GetGetMethod();

                cursor.Index += 2;
                cursor.Emit(OpCodes.Ldloca, 9);
                cursor.Emit(OpCodes.Call, getCurrentMethod);
                cursor.Index -= 2;
                // should render?
                cursor.EmitDelegate<Func<Entity, int>>(entity =>
                {
                    if (entity is CustomSeekerBarrier barrier)
                    {
                        return barrier.isDisabled ? 0 : 1;
                    }
                
                    return 1;
                });
                ILLabel skipRenderLabel= cursor.DefineLabel();
                cursor.Emit(OpCodes.Brfalse, skipRenderLabel);

                cursor.TryGotoNext(ins => ins.MatchBrtrue(out ILLabel _));
                cursor.Index -= 2;
                cursor.MarkLabel(skipRenderLabel);
            }
        }
        
        private static void onSeekerUpdate(ILContext il) {
            onSeekerOrJellyUpdate(il, seekerBarrier => seekerBarrier.killSeekers);
        }

        private static void onJellyUpdate(ILContext il) {
            onSeekerOrJellyUpdate(il, seekerBarrier => seekerBarrier.killJellyfish);
        }
        
        private static void onHoldableContainerUpdate(ILContext il) {
            onHoldableUpdate(il, (seekerBarrier, container) =>
            {
                bool slowFall = new DynamicData(container).Get<bool>("slowFall");
                return slowFall && seekerBarrier.killJellyfish  || !slowFall && seekerBarrier.killHoldableContainerNonSlowFall;
            });
        }
        
        private static void onHoldableUpdate(ILContext il, Func<CustomSeekerBarrier, HoldableContainer, bool> collideCondition) {
            ILCursor cursor = new ILCursor(il);
            cursor.TryGotoNext(instr => instr.MatchStloc(9));
            if (cursor.TryGotoNext(instr => instr.MatchLdcI4(1), instr => instr.MatchStfld<Entity>("Collidable"))) {
                Logger.Log("MaxHelpingHand/CustomSeekerBarrier", $"Disabling collision on seeker barriers at {cursor.Index} in IL for {il.Method.Name}");

                cursor.Emit(OpCodes.Dup);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Index++;
                cursor.EmitDelegate<Func<Entity, HoldableContainer, bool, bool>>((entity, holdableContainer, orig) =>
                {
                    if (entity is CustomSeekerBarrier seekerBarrier)
                    {
                        return collideCondition(seekerBarrier, holdableContainer) && !seekerBarrier.isDisabled;
                    }
                    return orig;
                });
            }
        }

        private static void onSeekerOrJellyUpdate(ILContext il, Func<CustomSeekerBarrier, bool> collideCondition) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchLdcI4(1), instr => instr.MatchStfld<Entity>("Collidable"))) {
                Logger.Log("MaxHelpingHand/CustomSeekerBarrier", $"Disabling collision on seeker barriers at {cursor.Index} in IL for {il.Method.Name}");

                cursor.Emit(OpCodes.Dup);
                cursor.Index++;
                cursor.EmitDelegate<Func<Entity, bool, bool>>((entity, orig) => {
                    if (entity is CustomSeekerBarrier seekerBarrier) {
                        return collideCondition(seekerBarrier) && !seekerBarrier.isDisabled;
                    }
                    return false;
                });
            }
        }

        internal class Renderer : SeekerBarrierRenderer {
            internal Color color;
            internal float transparency;
        }

        private SeekerBarrierRenderer renderer;

        internal Color particleColor;
        internal float particleTransparency;
        internal float particleDirection;
        internal bool wavy;

        private bool killSeekers;
        private bool killJellyfish;
        
        private bool killHoldableContainerNonSlowFall;
        private string disableIfFlag;
        private bool isDisabled;
        private static ILHook eeveeHoldableContainerUpdateHook;

        public CustomSeekerBarrier(EntityData data, Vector2 offset) : base(data, offset) {
            renderer = new Renderer() {
                Tag = Tags.TransitionUpdate, // get rid of the Global tag
                Depth = 1, // vanilla is 0, this makes it dependent on loading order
                color = Calc.HexToColor(data.Attr("color", "FFFFFF")),
                transparency = data.Float("transparency", 0.15f)
            };

            particleColor = Calc.HexToColor(data.Attr("particleColor", "FFFFFF"));
            particleTransparency = data.Float("particleTransparency", 0.5f);
            particleDirection = data.Float("particleDirection", 0f);
            wavy = data.Bool("wavy", defaultValue: true);

            killSeekers = data.Bool("killSeekers", defaultValue: true);
            killJellyfish = data.Bool("killJellyfish", defaultValue: true);
            
            disableIfFlag = data.Attr("disableIfFlag", "CustomSeekerBarrierDisabledFlag");
            killHoldableContainerNonSlowFall = data.Bool("killHoldableContainerNonSlowFall", false);
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            // track the barrier on our own renderer, instead of the vanilla global one.
            scene.Tracker.GetEntity<SeekerBarrierRenderer>().Untrack(this);
            scene.Add(renderer);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);

            renderer.Track(this);

            if (!SeekerBarrierColorController.HasControllerOnNextScreen() && scene.Entities.ToAdd.OfType<SeekerBarrierColorController>().Count() == 0) {
                // there is no seeker barrier color controller and we're not already adding one :pensive:
                // we need one, because that's the one tweaking the barriers.
                scene.Add(new SeekerBarrierColorController(new EntityData(), Vector2.Zero));
            }
        }

        public override void Update()
        {
            base.Update();
            Session session = SceneAs<Level>().Session;
            if (!isDisabled && session.GetFlag(disableIfFlag))
            {
                isDisabled = true;
                renderer.Untrack(this);
                Visible = false;
            }
            else if (isDisabled && !session.GetFlag(disableIfFlag))
            {
                isDisabled = false;
                renderer.Track(this);
                Visible = true;
            }
        }
    }
}
