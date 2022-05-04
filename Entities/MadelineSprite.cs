using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/MadelineSprite")]
    public class MadelineSprite : Player {
        private static ILHook hyperlineHook;
        private static MethodInfo moreDashelinePlayerUpdateHook;

        public static void Load() {
            IL.Celeste.Player.UpdateHair += onUpdateHair;
        }

        public static void Unload() {
            IL.Celeste.Player.UpdateHair -= onUpdateHair;

            hyperlineHook?.Dispose();
            hyperlineHook = null;
        }

        public static void HookMods() {
            if (hyperlineHook == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "Hyperline", Version = new Version(0, 2, 4) })) {
                Type hyperlineModule = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "Celeste.Mod.Hyperline.Hyperline")?.GetType();
                hyperlineHook = new ILHook(hyperlineModule.GetMethod("GetHairColor"), modHyperlineHairFlash);
            }

            if (moreDashelinePlayerUpdateHook == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "MoreDasheline", Version = new Version(1, 6, 5) })) {
                moreDashelinePlayerUpdateHook = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "MoreDasheline.MoreDashelineModule")
                    .GetType().GetMethod("Player_Update", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        private static void onUpdateHair(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("get_MaxDashes"))) {
                Logger.Log("MaxHelpingHand/MadelineSprite", $"Modding hair color on Prologue inventory at {cursor.Index} in IL for Player.UpdateHair");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<int, Player, int>>((orig, self) => {
                    if (self is MadelineSprite) {
                        // pretend the player has 1 dash inventory.
                        return 1;
                    }
                    return orig;
                });
            }

            cursor.Index = 0;

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCall<Engine>("get_DeltaTime"))) {
                Logger.Log("MaxHelpingHand/MadelineSprite", $"Making lerps instant at {cursor.Index} in IL for Player.UpdateHair");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<float, Player, float>>((orig, self) => {
                    if (self is MadelineSprite) {
                        // pretend delta time is 1 second, so that the lerp ends right away.
                        return 1;
                    }
                    return orig;
                });
            }
        }

        private static void modHyperlineHairFlash(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.OpCode == OpCodes.Call && (instr.Operand as MethodReference).Name == "IsFlash")) {
                Logger.Log("MaxHelpingHand/MadelineSprite", $"Hooking Hyperline hair flash at {cursor.Index} in IL for Hyperline.GetHairColor");

                cursor.Emit(OpCodes.Ldarg_1);
                cursor.EmitDelegate<Func<bool, PlayerHair, bool>>((orig, self) => {
                    if (self.Entity is MadelineSprite) {
                        // no flashing!
                        return false;
                    }
                    return orig;
                });
            }
        }

        private readonly int dashCount;

        private float windHairTimer = 0f;
        private float windTimeout = 0f;

        public MadelineSprite(EntityData data, Vector2 offset) : base(data.Position + offset, data.Bool("hasBackpack") ? PlayerSpriteMode.Madeline : PlayerSpriteMode.MadelineNoBackpack) {
            if (data.Bool("left")) {
                Facing = Facings.Left;
            }

            dashCount = data.Int("dashCount");

            // cut off the piece of code that makes Madeline face left depending on her position in the room
            IntroType = IntroTypes.None;

            // bigger hair size if we have more than 1 dash
            if (dashCount > 1) {
                Sprite.HairCount++;
                Hair.Nodes.Add(Vector2.Zero);
            }

            // do not react to anything, including wind
            Collidable = false;
            Remove(Get<WindMover>());
        }

        public override void Added(Scene scene) {
            base.Added(scene);

            Dashes = dashCount;

            // invoke More Dasheline if it is installed, to let it set hair color.
            if (moreDashelinePlayerUpdateHook != null) {
                EverestModule moreDashelineModule = Everest.Modules.FirstOrDefault(module => module.GetType().ToString() == "MoreDasheline.MoreDashelineModule");
                On.Celeste.Player.orig_Update fakeOrig = (player) => { };
                moreDashelinePlayerUpdateHook.Invoke(moreDashelineModule, new object[] { fakeOrig, this });
            }

            // initialize the hair color.
            new DynData<Player>(this)["lastDashes"] = dashCount;
            UpdateHair(true);
        }

        public override void Update() {
            // we ONLY want to update the sprite and hair.
            Sprite.Update();
            Hair.Update();

            // replicate the hair updating that happens in Update()

            if (windTimeout > 0f) {
                windTimeout -= Engine.DeltaTime;
            }

            Vector2 wind = Engine.DeltaTime * SceneAs<Level>().Wind;
            if (wind != Vector2.Zero) {
                windTimeout = 0.2f;
            }

            if (windTimeout > 0f && wind.X != 0f) {
                windHairTimer += Engine.DeltaTime * 8f;
                Hair.StepPerSegment = new Vector2(wind.X * 5f, (float) Math.Sin(windHairTimer));
                Hair.StepInFacingPerSegment = 0f;
                Hair.StepApproach = 128f;
                Hair.StepYSinePerSegment = 0f;
            } else if (Dashes > 1) {
                Hair.StepPerSegment = new Vector2((float) Math.Sin(Scene.TimeActive * 2f) * 0.7f - ((int) Facing * 3), (float) Math.Sin(Scene.TimeActive * 1f));
                Hair.StepInFacingPerSegment = 0f;
                Hair.StepApproach = 90f;
                Hair.StepYSinePerSegment = 1f;
                Hair.StepPerSegment.Y += wind.Y * 2f;
            } else {
                Hair.StepPerSegment = new Vector2(0f, 2f);
                Hair.StepInFacingPerSegment = 0.5f;
                Hair.StepApproach = 64f;
                Hair.StepYSinePerSegment = 0f;
                Hair.StepPerSegment.Y += wind.Y * 0.5f;
            }
        }
    }
}
