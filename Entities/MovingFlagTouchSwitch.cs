using Celeste.Mod.Entities;
using Celeste.Mod.MaxHelpingHand.Module;
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
    // this instanciates a moving touch switch from Outback Helper, but applies hooks and DynData to make it act like a flag touch switch.
    [CustomEntity("MaxHelpingHand/MovingFlagTouchSwitch")]
    public static class MovingFlagTouchSwitch {
        private static Type movingTouchSwitchType;
        private static FieldInfo movingTouchSwitchIcon;
        private static Type movingTouchSwitchStateMachineType;

        private static ILHook hookMovingTouchSwitchColor;

        public static void HookMods() {
            if (hookMovingTouchSwitchColor == null && Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "OutbackHelper", Version = new Version(1, 4, 0) })) {
                movingTouchSwitchType = Everest.Modules.First(module => module.GetType().ToString() == "Celeste.Mod.OutbackHelper.OutbackModule")
                    .GetType().Assembly.GetType("Celeste.Mod.OutbackHelper.MovingTouchSwitch");
                movingTouchSwitchIcon = movingTouchSwitchType.GetField("icon", BindingFlags.NonPublic | BindingFlags.Instance);

                MethodInfo movingTouchSwitchRoutine = movingTouchSwitchType.GetMethod("TriggeredSwitch", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget();
                movingTouchSwitchStateMachineType = movingTouchSwitchRoutine.DeclaringType;
                hookMovingTouchSwitchColor = new ILHook(movingTouchSwitchRoutine, modMovingTouchSwitchColor);

                On.Celeste.Switch.Activate += onSwitchActivate;
                On.Celeste.TouchSwitch.Update += onTouchSwitchUpdate;
            }
        }

        public static void Unload() {
            movingTouchSwitchType = null;
            movingTouchSwitchIcon = null;
            movingTouchSwitchStateMachineType = null;

            hookMovingTouchSwitchColor?.Dispose();
            hookMovingTouchSwitchColor = null;

            On.Celeste.Switch.Activate -= onSwitchActivate;
            On.Celeste.TouchSwitch.Update -= onTouchSwitchUpdate;
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
                entityData.Position = nodes[nodes.Length - 1];
                FlagTouchSwitch flagTouchSwitch = new FlagTouchSwitch(entityData, offset);
                entityData.Position = origPosition;
                return flagTouchSwitch;
            } else {
                // build the moving touch switch
                TouchSwitch movingTouchSwitch = (TouchSwitch) Activator.CreateInstance(movingTouchSwitchType,
                    new object[] { entityData.NodesOffset(offset), entityData, offset });

                // save its attributes as DynData
                DynData<TouchSwitch> switchData = new DynData<TouchSwitch>(movingTouchSwitch);
                switchData["flag"] = entityData.Attr("flag");
                switchData["id"] = entityData.ID;
                switchData["persistent"] = entityData.Bool("persistent", false);
                switchData["movingColor"] = Calc.HexToColor(entityData.Attr("movingColor", "FF8080"));

                // these attributes actually exist in TouchSwitch and as such, they work!
                switchData["inactiveColor"] = Calc.HexToColor(entityData.Attr("inactiveColor", "5FCDE4"));
                switchData["activeColor"] = Calc.HexToColor(entityData.Attr("activeColor", "FFFFFF"));
                switchData["finishColor"] = Calc.HexToColor(entityData.Attr("finishColor", "F141DF"));
                switchData["P_RecoloredFire"] = new ParticleType(TouchSwitch.P_Fire) {
                    Color = switchData.Get<Color>("finishColor")
                };

                // set up the icon
                string iconAttribute = entityData.Attr("icon", "vanilla");
                Sprite icon = new Sprite(GFX.Game, iconAttribute == "vanilla" ? "objects/touchswitch/icon" : $"objects/MaxHelpingHand/flagTouchSwitch/{iconAttribute}/icon");
                icon.Add("idle", "", 0f, default(int));
                icon.Add("spin", "", 0.1f, new Chooser<string>("spin", 1f), 0, 1, 2, 3, 4, 5);
                icon.Play("spin");
                icon.Color = switchData.Get<Color>("inactiveColor");
                icon.CenterOrigin();
                movingTouchSwitch.Remove(movingTouchSwitch.Get<Sprite>());
                movingTouchSwitch.Add(icon);
                movingTouchSwitchIcon.SetValue(movingTouchSwitch, icon);
                switchData["icon"] = icon;

                // collect the list of flag touch switches in the room as soon as the entity is awake, like regular flag touch switches.
                movingTouchSwitch.Add(new TouchSwitchListAttacher(entityData.Attr("flag")));

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

                DynData<TouchSwitch> data = new DynData<TouchSwitch>((TouchSwitch) Entity);

                // get all flag touch switches in the room.
                List<FlagTouchSwitch> allTouchSwitchesInRoom = Scene.Tracker.GetEntities<FlagTouchSwitch>()
                    .FindAll(touchSwitch => (touchSwitch as FlagTouchSwitch)?.Flag == flag).OfType<FlagTouchSwitch>().ToList();
                List<TouchSwitch> allMovingFlagTouchSwitchesInRoom = Scene.Entities.OfType<TouchSwitch>()
                    .Where(touchSwitch =>
                        touchSwitch.GetType().ToString() == "Celeste.Mod.OutbackHelper.MovingTouchSwitch" &&
                        new DynData<TouchSwitch>(touchSwitch).Data.ContainsKey("flag") &&
                        new DynData<TouchSwitch>(touchSwitch).Get<string>("flag") == flag).ToList();

                // store them in DynData.
                data["allTouchSwitchesInRoom"] = allTouchSwitchesInRoom;
                data["allMovingFlagTouchSwitchesInRoom"] = allMovingFlagTouchSwitchesInRoom;
            }

            public override void Update() {
                RemoveSelf();
            }
        }

        private static void modMovingTouchSwitchColor(ILContext il) {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchNewobj<Color>())) {
                Logger.Log("MaxHelpingHand/MovingFlagTouchSwitch", $"Customizing moving flag touch switch color at {cursor.Index} in IL for MovingTouchSwitch.TriggeredSwitch");

                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldfld, movingTouchSwitchStateMachineType.GetField("<>4__this"));
                cursor.EmitDelegate<Func<Color, TouchSwitch, Color>>((orig, self) => {
                    DynData<TouchSwitch> data = new DynData<TouchSwitch>(self);
                    if (data.Data.ContainsKey("movingColor")) {
                        return data.Get<Color>("movingColor");
                    }
                    return orig;
                });
            }
        }

        private static bool onSwitchActivate(On.Celeste.Switch.orig_Activate orig, Switch self) {
            if (self.Entity.GetType().ToString() == "Celeste.Mod.OutbackHelper.MovingTouchSwitch") {
                DynData<TouchSwitch> selfData = new DynData<TouchSwitch>((TouchSwitch) self.Entity);
                if (selfData.Data.ContainsKey("flag")) {
                    DynData<Switch> selfSwitch = new DynData<Switch>(self);
                    string flag = selfData.Get<string>("flag");
                    Level level = self.Entity.SceneAs<Level>();

                    // do what the regular Switch.Activate() method does
                    if (self.Finished || self.Activated) {
                        return false;
                    }
                    selfSwitch["Activated"] = true;
                    if (self.OnActivate != null) {
                        self.OnActivate();
                    }

                    // use the same logic as flag touch switches to determine if the group is complete.
                    return FlagTouchSwitch.HandleCollectedFlagTouchSwitch(flag, inverted: false, selfData.Get<bool>("persistent"), level, selfData.Get<int>("id"),
                        selfData.Get<List<FlagTouchSwitch>>("allTouchSwitchesInRoom"), selfData.Get<List<TouchSwitch>>("allMovingFlagTouchSwitchesInRoom"), () => { });
                }
            }

            // if we are here, it means we aren't dealing with a flag moving touch switch.
            // so, we want regular behavior!
            return orig(self);
        }

        private static void onTouchSwitchUpdate(On.Celeste.TouchSwitch.orig_Update orig, TouchSwitch self) {
            if (self.GetType().ToString() == "Celeste.Mod.OutbackHelper.MovingTouchSwitch") {
                DynData<TouchSwitch> selfData = new DynData<TouchSwitch>(self);
                if (selfData.Data.ContainsKey("P_RecoloredFire")) {
                    ParticleType oldParticle = TouchSwitch.P_Fire;
                    TouchSwitch.P_Fire = selfData.Get<ParticleType>("P_RecoloredFire");
                    orig(self);
                    TouchSwitch.P_Fire = oldParticle;
                    return;
                }
            }

            orig(self);
        }
    }
}
