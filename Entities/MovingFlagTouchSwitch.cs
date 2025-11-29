using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.MaxHelpingHand.Entities {
    // this instanciates a moving touch switch from Outback Helper, but applies hooks to make it act like a flag touch switch.
    [CustomEntity("MaxHelpingHand/MovingFlagTouchSwitch")]
    public static class MovingFlagTouchSwitch {
        private static Type movingTouchSwitchType;
        private static FieldInfo movingTouchSwitchIcon;
        private static Type movingTouchSwitchStateMachineType;

        private static ILHook hookMovingTouchSwitch;

        internal static readonly Dictionary<Entity, Dictionary<string, object>> flagMapping = new Dictionary<Entity, Dictionary<string, object>>();

        public static void HookMods() {
            if (hookMovingTouchSwitch == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "OutbackHelper", Version = new Version(1, 4, 0) })) {
                movingTouchSwitchType = Everest.Modules.First(module => module.GetType().ToString() == "Celeste.Mod.OutbackHelper.OutbackModule")
                    .GetType().Assembly.GetType("Celeste.Mod.OutbackHelper.MovingTouchSwitch");
                movingTouchSwitchIcon = movingTouchSwitchType.GetField("icon", BindingFlags.NonPublic | BindingFlags.Instance);

                MethodInfo movingTouchSwitchRoutine = movingTouchSwitchType.GetMethod("TriggeredSwitch", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
                movingTouchSwitchStateMachineType = movingTouchSwitchRoutine.DeclaringType;
                hookMovingTouchSwitch = new ILHook(movingTouchSwitchRoutine, modMovingTouchSwitchColor);

                On.Celeste.Switch.Activate += onSwitchActivate;
                On.Celeste.TouchSwitch.Update += onTouchSwitchUpdate;
                On.Monocle.Entity.Removed += onEntityRemoved;
                On.Celeste.Level.End += onLevelEnd;
            }
        }

        public static void Unload() {
            movingTouchSwitchType = null;
            movingTouchSwitchIcon = null;
            movingTouchSwitchStateMachineType = null;

            hookMovingTouchSwitch?.Dispose();
            hookMovingTouchSwitch = null;

            On.Celeste.Switch.Activate -= onSwitchActivate;
            On.Celeste.TouchSwitch.Update -= onTouchSwitchUpdate;
            On.Monocle.Entity.Removed -= onEntityRemoved;
            On.Celeste.Level.End -= onLevelEnd;
        }

        private static void onEntityRemoved(On.Monocle.Entity.orig_Removed orig, Entity self, Scene scene) {
            orig(self, scene);
            flagMapping.Remove(self);
        }

        private static void onLevelEnd(On.Celeste.Level.orig_End orig, Level self) {
            orig(self);
            flagMapping.Clear();
        }

        private class ErrorSpawner : Entity {
            public override void Update() {
                // error postcards are nicer than crashes!
                Audio.SetMusic(null);
                LevelEnter.ErrorMessage = "{big}Oops!{/big}{n}To use {# F94A4A}Moving Flag Touch Switches{#}, you need to have {# d678db}Outback Helper{#} installed!";
                LevelEnter.Go(new Session(SceneAs<Level>().Session.Area), fromSaveData: false);
            }
        }

        public static Entity Load(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
            if (!Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "OutbackHelper", Version = new Version(1, 4, 0) })) {
                return new ErrorSpawner();
            }

            string flag = entityData.Attr("flag");
            if (level.Session.GetFlag(flag) || level.Session.GetFlag(flag + "_switch" + entityData.ID)) {
                // moving touch switches can't be persistent, but we can very much spawn a flag touch switch instead!
                Vector2[] nodes = entityData.Nodes;
                Vector2 origPosition = entityData.Position;
                if (nodes.Length != 0) entityData.Position = nodes[nodes.Length - 1];
                FlagTouchSwitch flagTouchSwitch = new FlagTouchSwitch(entityData, offset);
                entityData.Position = origPosition;
                return flagTouchSwitch;
            } else {
                // build the moving touch switch
                TouchSwitch movingTouchSwitch = (TouchSwitch) Activator.CreateInstance(movingTouchSwitchType,
                    new object[] { entityData.NodesOffset(offset), entityData, offset });

                // save its attributes
                Dictionary<string, object> switchData = new Dictionary<string, object>();
                flagMapping[movingTouchSwitch] = switchData;
                switchData["flag"] = entityData.Attr("flag");
                switchData["id"] = entityData.ID;
                switchData["persistent"] = entityData.Bool("persistent", false);
                switchData["movingColor"] = Calc.HexToColor(entityData.Attr("movingColor", "FF8080"));
                switchData["movingDelay"] = entityData.Float("movingDelay", 0.8f);
                switchData["hideIfFlag"] = entityData.Attr("hideIfFlag");

                // these attributes actually exist in TouchSwitch and as such, they work!
                switchData["inactiveColor"] = Calc.HexToColor(entityData.Attr("inactiveColor", "5FCDE4"));
                switchData["activeColor"] = Calc.HexToColor(entityData.Attr("activeColor", "FFFFFF"));
                switchData["finishColor"] = Calc.HexToColor(entityData.Attr("finishColor", "F141DF"));
                switchData["P_RecoloredFire"] = new ParticleType(TouchSwitch.P_Fire) {
                    Color = (Color) switchData["finishColor"]
                };

                // set up the icon
                string iconAttribute = entityData.Attr("icon", "vanilla");
                Sprite icon = new Sprite(GFX.Game, iconAttribute == "vanilla" ? "objects/touchswitch/icon" : $"objects/MaxHelpingHand/flagTouchSwitch/{iconAttribute}/icon");
                icon.Add("idle", "", 0f, default(int));
                icon.Add("spin", "", 0.1f, new Chooser<string>("spin", 1f), 0, 1, 2, 3, 4, 5);
                icon.Play("spin");
                icon.Color = (Color) switchData["inactiveColor"];
                icon.CenterOrigin();
                movingTouchSwitch.Remove(movingTouchSwitch.Get<Sprite>());
                movingTouchSwitch.Add(icon);
                movingTouchSwitchIcon.SetValue(movingTouchSwitch, icon);
                switchData["icon"] = icon;

                // collect the list of flag touch switches in the room as soon as the entity is awake, like regular flag touch switches.
                movingTouchSwitch.Add(new TouchSwitchListAttacher(entityData.Attr("flag")));

                // also make sure we listen to any change in the hideIfFlag's value to disable the moving touch switch.
                string hideIfFlag = entityData.Attr("hideIfFlag");
                if (!string.IsNullOrEmpty(hideIfFlag)) {
                    movingTouchSwitch.Add(new HiddenListener(hideIfFlag));
                }

                return movingTouchSwitch;
            }
        }

        private class TouchSwitchListAttacher : Component {
            private string flag;

            public TouchSwitchListAttacher(string flag) : base(true, false) {
                this.flag = flag;
            }

            public override void EntityAwake() {
                base.EntityAwake();

                Dictionary<string, object> data = flagMapping[(TouchSwitch) Entity];

                // get all flag touch switches in the room.
                List<FlagTouchSwitch> allTouchSwitchesInRoom = Scene.Tracker.GetEntities<FlagTouchSwitch>()
                    .FindAll(touchSwitch => (touchSwitch as FlagTouchSwitch)?.Flag == flag).OfType<FlagTouchSwitch>().ToList();
                List<TouchSwitch> allMovingFlagTouchSwitchesInRoom = Scene.Entities.OfType<TouchSwitch>()
                    .Where(touchSwitch =>
                        touchSwitch.GetType().ToString() == "Celeste.Mod.OutbackHelper.MovingTouchSwitch" &&
                        flagMapping.TryGetValue(touchSwitch, out Dictionary<string, object> data) &&
                        (string) data["flag"] == flag).ToList();

                data["allTouchSwitchesInRoom"] = allTouchSwitchesInRoom;
                data["allMovingFlagTouchSwitchesInRoom"] = allMovingFlagTouchSwitchesInRoom;
            }

            public override void Update() {
                RemoveSelf();
            }
        }

        private class HiddenListener : Component {
            private readonly string hideIfFlag;

            public HiddenListener(string hideIfFlag) : base(active: true, visible: false) {
                this.hideIfFlag = hideIfFlag;
            }

            public override void Update() {
                if (Entity != null && (Entity.Scene as Level).Session.GetFlag(hideIfFlag)) {
                    // disable the entity entirely, and spawn another entity to continue monitoring the flag
                    Entity.Visible = Entity.Active = Entity.Collidable = false;
                    Scene.Add(new FlagTouchSwitch.ResurrectOnFlagDisableController { Entity = Entity, Flag = hideIfFlag });
                    return;
                }
            }
        }

        private static void modMovingTouchSwitchColor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchNewobj<Color>())) {
                Logger.Log("MaxHelpingHand/MovingFlagTouchSwitch", $"Customizing moving flag touch switch color at {cursor.Index} in IL for MovingTouchSwitch.TriggeredSwitch");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, movingTouchSwitchStateMachineType.GetField("<>4__this"));
                cursor.EmitDelegate<Func<Color, TouchSwitch, Color>>(modTouchSwitchColor);
            }

            cursor.Index = 0;

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.8f))) {
                Logger.Log("MaxHelpingHand/MovingFlagTouchSwitch", $"Customizing moving flag touch switch delay at {cursor.Index} in IL for MovingTouchSwitch.TriggeredSwitch");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, movingTouchSwitchStateMachineType.GetField("<>4__this"));
                cursor.EmitDelegate<Func<float, TouchSwitch, float>>(modTouchSwitchDelay);
            }
        }

        private static Color modTouchSwitchColor(Color orig, TouchSwitch self) {
            if (flagMapping.TryGetValue(self, out Dictionary<string, object> map)) {
                return (Color) map["movingColor"];
            }
            return orig;
        }

        private static float modTouchSwitchDelay(float orig, TouchSwitch self) {
            if (flagMapping.TryGetValue(self, out Dictionary<string, object> map)) {
                return (float) map["movingDelay"];
            }
            return orig;
        }

        private static bool onSwitchActivate(On.Celeste.Switch.orig_Activate orig, Switch self) {
            if (self.Entity.GetType().ToString() == "Celeste.Mod.OutbackHelper.MovingTouchSwitch") {
                if (flagMapping.TryGetValue(self.Entity, out Dictionary<string, object> map)) {
                    string flag = (string) map["flag"];
                    Level level = self.Entity.SceneAs<Level>();

                    // do what the regular Switch.Activate() method does
                    if (self.Finished || self.Activated) {
                        return false;
                    }
                    self.Activated = true;
                    if (self.OnActivate != null) {
                        self.OnActivate();
                    }

                    // use the same logic as flag touch switches to determine if the group is complete.
                    return FlagTouchSwitch.HandleCollectedFlagTouchSwitch(flag, inverted: false, (bool) map["persistent"], level, (int) map["id"],
                        (List<FlagTouchSwitch>) map["allTouchSwitchesInRoom"], (List<TouchSwitch>) map["allMovingFlagTouchSwitchesInRoom"], () => { });
                }
            }

            // if we are here, it means we aren't dealing with a flag moving touch switch.
            // so, we want regular behavior!
            return orig(self);
        }

        private static void onTouchSwitchUpdate(On.Celeste.TouchSwitch.orig_Update orig, TouchSwitch self) {
            if (self.GetType().ToString() == "Celeste.Mod.OutbackHelper.MovingTouchSwitch") {
                if (flagMapping.TryGetValue(self, out Dictionary<string, object> map)) {
                    ParticleType oldParticle = TouchSwitch.P_Fire;
                    TouchSwitch.P_Fire = (ParticleType) map["P_RecoloredFire"];
                    orig(self);
                    TouchSwitch.P_Fire = oldParticle;
                    return;
                }
            }

            orig(self);
        }

        internal static bool IsHidden(TouchSwitch touchSwitch) {
            if (flagMapping.TryGetValue(touchSwitch, out Dictionary<string, object> map)) {
                string hideIfFlag = (string) map["hideIfFlag"];
                return !string.IsNullOrEmpty(hideIfFlag) && (touchSwitch.Scene as Level).Session.GetFlag(hideIfFlag);
            }
            return false;
        }
    }
}
