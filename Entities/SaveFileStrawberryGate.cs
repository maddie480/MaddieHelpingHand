using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
using LunaticHelper;
using Microsoft.Xna.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/SaveFileStrawberryGate")]
    public static class SaveFileStrawberryGate {
        private enum CountFrom { Side, Chapter, Campaign, SaveFile };

        private static MethodInfo openAmount;
        private static FieldInfo heartAlpha;

        private static ILHook hookStrawberryGateRoutine = null;
        private static ILHook hookStrawberryGateAdded = null;
        private static Hook hookStrawberryGateRender = null;
        private static ILHook fixStrawberryGateRender = null;

        private static FieldInfo strawberryGateIcon;

        private static MTexture[] numbers;

        public static void HookMods() {
            if (hookStrawberryGateRoutine == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "LunaticHelper", Version = new Version(1, 1, 1) })) {
                hookLunaticHelper();
            }
        }
        public static void Initialize() {
            // extract numbers from the PICO-8 font that ships with the game.
            MTexture source = GFX.Game["pico8/font"];
            numbers = new MTexture[10];
            int index = 0;
            for (int i = 104; index < 4; i += 4) {
                numbers[index++] = source.GetSubtexture(i, 0, 3, 5);
            }
            for (int i = 0; index < 10; i += 4) {
                numbers[index++] = source.GetSubtexture(i, 6, 3, 5);
            }
        }

        private static void hookLunaticHelper() {
            MethodInfo strawberryGateRoutine = typeof(StrawberryGate).GetMethod("Routine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
            hookStrawberryGateRoutine = new ILHook(strawberryGateRoutine, modStrawberryGateRoutine);

            MethodInfo strawberryGateAdded = typeof(StrawberryGate).GetMethod("Added");
            hookStrawberryGateAdded = new ILHook(strawberryGateAdded, modStrawberryGateAdded);

            MethodInfo strawberryGateRender = typeof(StrawberryGate).GetMethod("Render");
            hookStrawberryGateRender = new Hook(strawberryGateRender, typeof(SaveFileStrawberryGate).GetMethod("modStrawberryGateRender", BindingFlags.NonPublic | BindingFlags.Static));
            fixStrawberryGateRender = new ILHook(strawberryGateRender, fixForStrawberryGateRender);

            strawberryGateIcon = typeof(StrawberryGate).GetField("icon", BindingFlags.NonPublic | BindingFlags.Instance);

            openAmount = typeof(StrawberryGate).GetMethod("get_openAmount", BindingFlags.NonPublic | BindingFlags.Instance);
            heartAlpha = typeof(StrawberryGate).GetField("heartAlpha", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static void Unload() {
            hookStrawberryGateRoutine?.Dispose();
            hookStrawberryGateRoutine = null;

            hookStrawberryGateAdded?.Dispose();
            hookStrawberryGateAdded = null;

            hookStrawberryGateRender?.Dispose();
            hookStrawberryGateRender = null;

            fixStrawberryGateRender?.Dispose();
            fixStrawberryGateRender = null;

            openAmount = null;
            heartAlpha = null;
        }

        private class ErrorSpawner : Entity {
            public override void Update() {
                // error postcards are nicer than crashes!
                Audio.SetMusic(null);
                LevelEnter.ErrorMessage = "{big}Oops!{/big}{n}To use {# F94A4A}Save File Strawberry Gates{#}, you need to have {# d678db}Lunatic Helper{#} installed!";
                LevelEnter.Go(new Session(SceneAs<Level>().Session.Area), fromSaveData: false);
            }
        }

        private class BerryCountModdingThingy : Component {
            public readonly CountFrom CountFrom;
            public readonly bool Persistent;
            public readonly int EntityID;
            public readonly bool ShowCounter;

            public BerryCountModdingThingy(CountFrom countFrom, bool persistent, int entityID, bool showCounter) : base(false, false) {
                CountFrom = countFrom;
                Persistent = persistent;
                EntityID = entityID;
                ShowCounter = showCounter;
            }
        }

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            if (!Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "LunaticHelper", Version = new Version(1, 1, 1) })) {
                return new ErrorSpawner();
            }
            return createStrawberryGate(entityData, offset);
        }

        private static Entity createStrawberryGate(EntityData data, Vector2 offset) {
            StrawberryGate gate = new StrawberryGate(data, offset) {
                new BerryCountModdingThingy(data.Enum<CountFrom>("countFrom"), data.Bool("persistent"), data.ID, data.Bool("showCounter"))
            };

            if (data.Bool("showCounter")) {
                strawberryGateIcon.SetValue(gate, GFX.Game.GetAtlasSubtextures("objects/MaxHelpingHand/strawberry_gate_no_icon"));
            }

            return gate;
        }

        private static void modStrawberryGateRoutine(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // mod the strawberry count the player has to pull it from whatever the "Count From" option says.
            while (cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdfld<Session>("Strawberries"),
                instr => instr.OpCode == OpCodes.Callvirt && (instr.Operand as MethodReference).Name == "get_Count")) {

                Logger.Log("MaxHelpingHand/SaveFileStrawberryGate", $"Modding strawberry count at {cursor.Index} in IL for StrawberryGate.Routine");

                // This is the convoluted way to retrieve "this" when you are inside a coroutine. You read from a hidden field called "<>4__this". Yeah.
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(StrawberryGate).GetMethod("Routine", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetStateMachineTarget().DeclaringType.GetField("<>4__this"));

                cursor.EmitDelegate<Func<int, StrawberryGate, int>>(modBerryCount);
            }

            cursor.Index = 0;

            // when the gate opens and this is recorded to the session, also record it to the save data if "Persistent" is checked.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdstr("opened_strawberrygate_"))
                && cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Session>("SetFlag"))) {

                Logger.Log("MaxHelpingHand/SaveFileStrawberryGate", $"Making strawberry gate opening persistent at {cursor.Index} in IL for StrawberryGate.Routine");

                // Retrieve "this" again
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, typeof(StrawberryGate).GetMethod("Routine", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetStateMachineTarget().DeclaringType.GetField("<>4__this"));

                cursor.EmitDelegate<Action<StrawberryGate>>(saveFilePersist);
            }
        }

        private static int modBerryCount(int orig, StrawberryGate self) {
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
        }

        private static void saveFilePersist(StrawberryGate self) {
            BerryCountModdingThingy thing = self.Get<BerryCountModdingThingy>();
            if (thing != null && thing.Persistent) {
                string sid = (self.Scene as Level).Session.Area.GetSID();

                if (!MaxHelpingHandModule.Instance.SaveData.OpenedSaveFileStrawberryGates.ContainsKey(sid)) {
                    MaxHelpingHandModule.Instance.SaveData.OpenedSaveFileStrawberryGates[sid] = new HashSet<int>();
                }
                MaxHelpingHandModule.Instance.SaveData.OpenedSaveFileStrawberryGates[sid].Add(thing.EntityID);
            }
        }

        private static void modStrawberryGateAdded(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            // if the gate was opened in that save and "Persistent" is checked, make it open even if the gate wasn't opened in the session yet.
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Session>("GetFlag"))) {
                Logger.Log("MaxHelpingHand/SaveFileStrawberryGate", $"Modding flag result at {cursor.Index} in IL for StrawberryGate.Added");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.EmitDelegate<Func<bool, StrawberryGate, bool>>(checkGateWasOpened);
            }
        }

        private static bool checkGateWasOpened(bool orig, StrawberryGate self) {
            BerryCountModdingThingy thing = self.Get<BerryCountModdingThingy>();
            if (thing == null) {
                return orig;
            }

            // if the strawberry gate is persistent, also check if it was already opened in the past.
            return orig || (thing.Persistent
                && MaxHelpingHandModule.Instance.SaveData.OpenedSaveFileStrawberryGates.TryGetValue((self.Scene as Level).Session.Area.GetSID(), out HashSet<int> openedIDs)
                && openedIDs.Contains(thing.EntityID));
        }

        private static void modStrawberryGateRender(Action<Entity> orig, Entity self) {
            orig(self);

            BerryCountModdingThingy thing = self.Get<BerryCountModdingThingy>();
            if (thing != null && thing.ShowCounter) {
                StrawberryGate gate = (StrawberryGate) self;

                Color color = (gate.Opened ? (Color.White * 0.25f) : Color.White) * (float) heartAlpha.GetValue(self);

                // draw the strawberry
                GFX.Game["objects/lunatichelper/strawberrygate/icon00"].DrawCentered(new Vector2(
                    gate.X + gate.Size * 0.5f,
                    gate.Y - (float) openAmount.Invoke(self, new object[0]) - 10
                ), color);

                // draw the counts
                int separatorWidth = gate.Requires.ToString().Length * 4 - 1;
                Vector2 anchor = new Vector2(
                    gate.X + gate.Size * 0.5f,
                    gate.Y + (float) openAmount.Invoke(self, new object[0]) + 5
                );
                drawNumber((int) gate.Counter, anchor, color);
                Draw.Line(anchor + new Vector2(-separatorWidth / 2 - 1, 5), anchor + new Vector2(separatorWidth / 2 + 2, 5), color);
                drawNumber(gate.Requires, anchor + new Vector2(0, 8), color);
            }
        }

        private static void fixForStrawberryGateRender(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            if (cursor.TryGotoNext(instr => instr.MatchCallvirt<MTexture>("DrawCentered")) && cursor.TryGotoPrev(MoveType.After, instr => instr.MatchCall<Vector2>("op_Addition"))) {
                Logger.Log("MaxHelpingHand/SaveFileStrawberryGate", $"Fixing strawberry gate offset at {cursor.Index} in IL for StrawberryGate.Render");
                cursor.EmitDelegate<Func<Vector2, Vector2>>(modStrawberryGateOffset);
            }
        }

        private static Vector2 modStrawberryGateOffset(Vector2 orig) {
            return new Vector2((float) Math.Round(orig.X), (float) Math.Floor(orig.Y));
        }

        private static void drawNumber(int numberI, Vector2 anchor, Color color) {
            string number = numberI.ToString();
            int totalWidth = number.Length * 4 - 1;

            for (int i = 0; i < number.Length; i++) {
                Vector2 position = anchor + new Vector2(-totalWidth / 2 + i * 4, 0);
                numbers[number.ToCharArray()[i] - '0'].Draw(position, new Vector2(0f, 0.5f), color);
            }
        }
    }
}
