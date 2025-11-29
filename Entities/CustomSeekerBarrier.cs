using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/CustomSeekerBarrier")]
    [TrackedAs(typeof(SeekerBarrier))]
    public class CustomSeekerBarrier : SeekerBarrier {
        private static ILHook eeveeHoldableContainerUpdateHook;
        private static FieldInfo holdableContainerSlowFall;

        public static void Load() {
            IL.Celeste.Seeker.Update += onSeekerUpdate;
            IL.Celeste.Glider.Update += onJellyUpdate;
            IL.Celeste.BloomRenderer.Apply += onBloomRendererApply;
        }

        public static void LoadMods() {
            using (new DetourConfigContext(new DetourConfig("MaddieHelpingHand_AfterAll").WithPriority(int.MaxValue)).Use()) {
                if (eeveeHoldableContainerUpdateHook == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "EeveeHelper", Version = new Version(1, 12, 2) })) {
                    Type holdableContainerType = Everest.Modules.First(mod => mod.GetType().ToString() == "Celeste.Mod.EeveeHelper.EeveeHelperModule")
                        .GetType().Assembly.GetType("Celeste.Mod.EeveeHelper.Entities.HoldableContainer");

                    eeveeHoldableContainerUpdateHook = new ILHook(
                        holdableContainerType.GetMethod("Update", BindingFlags.Public | BindingFlags.Instance),
                        onHoldableContainerUpdate
                    );

                    holdableContainerSlowFall = holdableContainerType.GetField("slowFall", BindingFlags.NonPublic | BindingFlags.Instance);
                }
            }
        }
        public static void Unload() {
            IL.Celeste.Seeker.Update -= onSeekerUpdate;
            IL.Celeste.Glider.Update -= onJellyUpdate;
            IL.Celeste.BloomRenderer.Apply -= onBloomRendererApply;

            eeveeHoldableContainerUpdateHook?.Dispose();
            eeveeHoldableContainerUpdateHook = null;
            holdableContainerSlowFall = null;
        }

        private static void onBloomRendererApply(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNextBestFit(
                MoveType.Before,
                ins => ins.MatchLdloca(9),
                ins => ins.MatchCall(out MethodReference _),
                ins => ins.MatchCallvirt(out MethodReference _),
                ins => ins.MatchCall(out MethodReference _),
                ins => ins.MatchCall(out MethodReference _)
            )) {

                MethodInfo getCurrentMethod = typeof(List<Entity>.Enumerator).GetProperty("Current").GetGetMethod();

                cursor.Index += 2;
                cursor.Emit(OpCodes.Ldloca, 9);
                cursor.Emit(OpCodes.Call, getCurrentMethod);
                cursor.Index -= 2;
                // should render?
                cursor.EmitDelegate<Func<Entity, int>>(seekerBarrierDisable);
                ILLabel skipRenderLabel = cursor.DefineLabel();
                cursor.Emit(OpCodes.Brfalse, skipRenderLabel);

                cursor.TryGotoNext(ins => ins.MatchBrtrue(out ILLabel _));
                cursor.Index -= 2;
                cursor.MarkLabel(skipRenderLabel);
            }
        }

        private static int seekerBarrierDisable(Entity entity) {
            if (entity is CustomSeekerBarrier barrier) {
                return barrier.isDisabled ? 0 : 1;
            }

            return 1;
        }

        private static void onSeekerUpdate(ILContext il) {
            onSeekerOrJellyUpdate(il, collideConditionSeekers);
        }
        private static void onJellyUpdate(ILContext il) {
            onSeekerOrJellyUpdate(il, collideConditionJellyfish);
        }

        private static bool collideConditionSeekers(Entity seekerBarrier, bool orig) {
            return seekerBarrier is CustomSeekerBarrier b ? b.killSeekers && !b.isDisabled : orig;
        }
        private static bool collideConditionJellyfish(Entity seekerBarrier, bool orig) {
            return seekerBarrier is CustomSeekerBarrier b ? b.killJellyfish && !b.isDisabled : orig;
        }

        private static void onHoldableContainerUpdate(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            cursor.TryGotoNext(instr => instr.MatchStloc(9));
            if (cursor.TryGotoNext(instr => instr.MatchLdcI4(1), instr => instr.MatchStfld<Entity>("Collidable"))) {
                Logger.Log("MaxHelpingHand/CustomSeekerBarrier", $"Disabling collision on seeker barriers at {cursor.Index} in IL for {il.Method.Name}");

                cursor.Emit(OpCodes.Dup);
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Index++;
                cursor.EmitDelegate<Func<Entity, Entity, bool, bool>>(modHoldableCollideCondition);
            }
        }

        private static bool modHoldableCollideCondition(Entity entity, Entity holdableContainer, bool orig) {
            if (entity is CustomSeekerBarrier seekerBarrier) {
                bool slowFall = (bool) holdableContainerSlowFall.GetValue(holdableContainer);
                return !seekerBarrier.isDisabled && (
                    (slowFall && seekerBarrier.killHoldableContainerSlowFall)
                    || (!slowFall && seekerBarrier.killHoldableContainerNonSlowFall));
            }
            return orig;
        }

        private static void onSeekerOrJellyUpdate(ILContext il, Func<Entity, bool, bool> collideCondition) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(instr => instr.MatchLdcI4(1), instr => instr.MatchStfld<Entity>("Collidable"))) {
                Logger.Log("MaxHelpingHand/CustomSeekerBarrier", $"Disabling collision on seeker barriers at {cursor.Index} in IL for {il.Method.Name}");

                cursor.Emit(OpCodes.Dup);
                cursor.Index++;
                cursor.EmitDelegate<Func<Entity, bool, bool>>(collideCondition);
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
        private bool killHoldableContainerSlowFall;
        private string disableIfFlag;
        private bool isDisabled;

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

            disableIfFlag = data.Attr("disableIfFlag");
            killHoldableContainerNonSlowFall = data.Bool("killHoldableContainerNonSlowFall", defaultValue: true);
            killHoldableContainerSlowFall = data.Bool("killHoldableContainerSlowFall", defaultValue: true);
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

        public override void Update() {
            base.Update();

            if (string.IsNullOrEmpty(disableIfFlag)) return;

            Session session = SceneAs<Level>().Session;
            if (!isDisabled && session.GetFlag(disableIfFlag)) {
                isDisabled = true;
                renderer.Untrack(this);
                Visible = false;
            } else if (isDisabled && !session.GetFlag(disableIfFlag)) {
                isDisabled = false;
                renderer.Track(this);
                Visible = true;
            }
        }
    }
}
