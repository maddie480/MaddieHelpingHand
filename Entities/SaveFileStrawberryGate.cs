using Celeste.Mod.Entities;
using LunaticHelper;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/SaveFileStrawberryGate")]
    public static class SaveFileStrawberryGate {
        private enum CountFrom { Side, Chapter, Campaign, SaveFile };

        private static ILHook hookStrawberryGateRoutine = null;

        public static void HookMods() {
            if (hookStrawberryGateRoutine == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "LunaticHelper", Version = new Version(1, 1, 1) })) {
                hookLunaticHelper();
            }
        }

        private static void hookLunaticHelper() {
            MethodInfo strawberryGateRoutine = typeof(StrawberryGate).GetMethod("Routine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
            hookStrawberryGateRoutine = new ILHook(strawberryGateRoutine, modStrawberryGateRoutine);
        }

        public static void Unload() {
            hookStrawberryGateRoutine?.Dispose();
            hookStrawberryGateRoutine = null;
        }

        private class ErrorSpawner : Entity {
            public override void Update() {
                // error postcards are nicer than crashes!
                Audio.SetMusic(null);
                LevelEnter.ErrorMessage = "{big}Oops!{/big}{n}To use {# F94A4A}Save File Strawberry Gates{#}, you need to have {# d678db}Lunatic Helper{#} installed!";
                LevelEnter.Go(new Session(SceneAs<Level>().Session.Area), fromSaveData: false);
            }
        }

        [Tracked]
        private class BerryCountModdingThingy : Component {
            public CountFrom CountFrom;

            public BerryCountModdingThingy(CountFrom countFrom) : base(false, false) {
                CountFrom = countFrom;
            }
        }

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            if (!Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "LunaticHelper", Version = new Version(1, 1, 1) })) {
                return new ErrorSpawner();
            }
            return createStrawberryGate(entityData, offset);
        }

        private static Entity createStrawberryGate(EntityData data, Vector2 offset) {
            return new StrawberryGate(data, offset) {
                new BerryCountModdingThingy(data.Enum<CountFrom>("countFrom"))
            };
        }

        private static void modStrawberryGateRoutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdfld<Session>("Strawberries"),
                instr => instr.OpCode == OpCodes.Callvirt && (instr.Operand as MethodReference).Name == "get_Count")) {

                Logger.Log("MaxHelpingHand/SaveFileStrawberryGate", $"Modding strawberry count at {cursor.Index} in IL for StrawberryGate.Routine");

                // This is the convoluted way to retrieve "this" when you are inside a coroutine. You read from a hidden field called "<>4__this". Yeah.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(StrawberryGate).GetMethod("Routine", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetStateMachineTarget().DeclaringType.GetField("<>4__this"));

                cursor.EmitDelegate<Func<int, StrawberryGate, int>>((orig, self) => {
                    BerryCountModdingThingy thing = self.Get<BerryCountModdingThingy>();
                    if (thing == null) {
                        return orig;
                    }

                    Session session = (self.Scene as Level).Session;
                    switch (thing.CountFrom) {
                        case CountFrom.Side:
                            return SaveData.Instance.GetAreaStatsFor(session.Area).Modes[(int) session.Area.Mode].TotalStrawberries;
                        case CountFrom.Chapter:
                            return SaveData.Instance.GetAreaStatsFor(session.Area).TotalStrawberries;
                        case CountFrom.Campaign:
                            return SaveData.Instance.GetLevelSetStatsFor(session.Area.GetLevelSet()).TotalStrawberries;
                        case CountFrom.SaveFile:
                            return SaveData.Instance.TotalStrawberries;
                        default:
                            throw new Exception("Invalid enum value " + thing.CountFrom + "!");
                    }
                });
            }
        }
    }
}
