using Celeste.Mod.Entities;
using Celeste.Pico8;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    [CustomEntity("MaxHelpingHand/Pico8FlagController")]
    public class Pico8FlagController : Entity {
        private readonly string flagOnComplete;
        private readonly string flagOnBerryCount;
        private readonly int berryCount;

        public Pico8FlagController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            flagOnComplete = data.Attr("flagOnComplete");
            flagOnBerryCount = data.Attr("flagOnBerryCount");
            berryCount = data.Int("berryCount");
        }


        private static bool completed;
        private static int berries;

        public static void Load() {
            IL.Celeste.Pico8.Classic.flag.draw += onPico8End;
            IL.Celeste.Pico8.Classic.fruit.update += onPico8Berry;
            IL.Celeste.Pico8.Classic.fly_fruit.update += onPico8Berry;
            On.Celeste.LevelLoader.LoadingThread += onLoadLevel;
        }

        public static void Unload() {
            IL.Celeste.Pico8.Classic.flag.draw -= onPico8End;
            IL.Celeste.Pico8.Classic.fruit.update -= onPico8Berry;
            IL.Celeste.Pico8.Classic.fly_fruit.update -= onPico8Berry;
            On.Celeste.LevelLoader.LoadingThread -= onLoadLevel;
        }

        private static void onLoadLevel(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            completed = false;
            berries = 0;
            Logger.Log(LogLevel.Debug, "MaxHelpingHand/Pico8FlagController", "PICO-8 state was reset");

            orig(self);
        }

        public override void SceneEnd(Scene scene) {
            base.SceneEnd(scene);

            completed = false;
            berries = 0;
            Logger.Log(LogLevel.Debug, "MaxHelpingHand/Pico8FlagController", "PICO-8 state was reset");
        }


        public override void SceneBegin(Scene scene) {
            base.SceneBegin(scene);

            Logger.Log(LogLevel.Debug, "MaxHelpingHand/Pico8FlagController", $"Checking PICO-8 state: completed (flag {flagOnComplete}) = {completed}, berries (flag {flagOnBerryCount}) = {berries}/{berryCount}");
            if (completed && !string.IsNullOrEmpty(flagOnComplete)) {
                Logger.Log(LogLevel.Debug, "MaxHelpingHand/Pico8FlagController", "PICO-8 complete flag triggered!");
                SceneAs<Level>().Session.SetFlag(flagOnComplete);
            }
            if (berries >= berryCount && !string.IsNullOrEmpty(flagOnBerryCount)) {
                Logger.Log(LogLevel.Debug, "MaxHelpingHand/Pico8FlagController", "Berry collection flag triggered!");
                SceneAs<Level>().Session.SetFlag(flagOnBerryCount);
            }
        }

        private static void onPico8End(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(instr => instr.MatchStfld<Classic.flag>("show"));
            cursor.EmitDelegate(() => {
                completed = true;
                Logger.Log(LogLevel.Debug, "MaxHelpingHand/Pico8FlagController", "Player completed PICO-8!");
            });
        }

        private static void onPico8Berry(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            cursor.GotoNext(instr => instr.MatchCall(typeof(Stats), "Increment"));

            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, typeof(Classic.ClassicObject).GetField("G"));
            cursor.Emit(OpCodes.Ldfld, typeof(Classic).GetField("got_fruit", BindingFlags.NonPublic | BindingFlags.Instance));
            cursor.EmitDelegate<Action<HashSet<int>>>(berryList => {
                berries = berryList.Count;
                Logger.Log(LogLevel.Debug, "MaxHelpingHand/Pico8FlagController", $"Player got {berries} berries in PICO-8");
            });
        }
    }
}
