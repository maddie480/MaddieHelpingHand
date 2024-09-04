using System;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand.Module {
    public class MaxHelpingHandMapDataProcessor : EverestMapDataProcessor {

        // the structure here is: FlagTouchSwitches[AreaSID][ModeID][flagName, inverted] = list of entity ids for flag touch switches / flag switch gates in this group on this map.
        public static Dictionary<string, List<Dictionary<KeyValuePair<string, bool>, List<EntityID>>>> FlagTouchSwitches = new Dictionary<string, List<Dictionary<KeyValuePair<string, bool>, List<EntityID>>>>();
        public static Dictionary<string, List<Dictionary<string, Dictionary<EntityID, bool>>>> FlagSwitchGates = new Dictionary<string, List<Dictionary<string, Dictionary<EntityID, bool>>>>();
        public static int DetectedSecretBerries = 0;
        private string levelName;

        // we want to match multi-room strawberry seeds with the strawberry that has the same name.
        private Dictionary<string, List<BinaryPacker.Element>> multiRoomStrawberrySeedsByName = new Dictionary<string, List<BinaryPacker.Element>>();
        private Dictionary<string, EntityID> multiRoomStrawberryIDsByName = new Dictionary<string, EntityID>();
        private Dictionary<string, BinaryPacker.Element> multiRoomStrawberriesByName = new Dictionary<string, BinaryPacker.Element>();

        public override Dictionary<string, Action<BinaryPacker.Element>> Init() {
            Logger.Log(LogLevel.Verbose, "MaxHelpingHand/MapDataProcessor", $"Initializing map data processor for {AreaKey.SID} / {AreaKey.Mode}!");

            Action<BinaryPacker.Element> flagSwitchGateHandler = flagSwitchGate => {
                string flag = flagSwitchGate.Attr("flag");
                Dictionary<string, Dictionary<EntityID, bool>> allSwitchGatesInMap = FlagSwitchGates[AreaKey.SID][(int) AreaKey.Mode];

                // if no dictionary entry exists for this flag, create one. otherwise, get it.
                Dictionary<EntityID, bool> entityIDs;
                if (!allSwitchGatesInMap.ContainsKey(flag)) {
                    entityIDs = new Dictionary<EntityID, bool>();
                    allSwitchGatesInMap[flag] = entityIDs;
                } else {
                    entityIDs = allSwitchGatesInMap[flag];
                }

                // add this flag switch gate to the dictionary.
                entityIDs.Add(new EntityID(levelName, flagSwitchGate.AttrInt("id")), flagSwitchGate.AttrBool("persistent"));

                Logger.Log(LogLevel.Verbose, "MaxHelpingHand/MapDataProcessor", $"area {AreaKey.SID} / mode {AreaKey.Mode} / room {levelName} has flag switch gate {flagSwitchGate.AttrInt("id")} with flag {flag} and persistent {flagSwitchGate.AttrBool("persistent")}");
            };

            Action<BinaryPacker.Element> flagTouchSwitchHandler = flagTouchSwitch => {
                string flag = flagTouchSwitch.Attr("flag");
                bool inverted = flagTouchSwitch.AttrBool("inverted", false);
                KeyValuePair<string, bool> key = new KeyValuePair<string, bool>(flag, inverted);
                Dictionary<KeyValuePair<string, bool>, List<EntityID>> allTouchSwitchesInMap = FlagTouchSwitches[AreaKey.SID][(int) AreaKey.Mode];

                // if no dictionary entry exists for this flag, create one. otherwise, get it.
                List<EntityID> entityIDs;
                if (!allTouchSwitchesInMap.ContainsKey(key)) {
                    entityIDs = new List<EntityID>();
                    allTouchSwitchesInMap[key] = entityIDs;
                } else {
                    entityIDs = allTouchSwitchesInMap[key];
                }

                // add this flag touch switch to the dictionary.
                entityIDs.Add(new EntityID(levelName, flagTouchSwitch.AttrInt("id")));

                Logger.Log(LogLevel.Verbose, "MaxHelpingHand/MapDataProcessor", $"area {AreaKey.SID} / mode {AreaKey.Mode} / room {levelName} has flag touch switch {flagTouchSwitch.AttrInt("id")} with flag {key}");
            };

            return new Dictionary<string, Action<BinaryPacker.Element>> {
                {
                    "level", level => {
                        // be sure to write the level name down.
                        levelName = level.Attr("name").Split(':')[0];
                        if (levelName.StartsWith("lvl_")) {
                            levelName = levelName.Substring(4);
                        }
                    }
                },
                {
                    "entity:MaxHelpingHand/FlagTouchSwitch", flagTouchSwitchHandler
                },
                {
                    "entity:MaxHelpingHand/MovingFlagTouchSwitch", flagTouchSwitchHandler
                },
                {
                    "entity:MaxHelpingHand/FlagSwitchGate", flagSwitchGateHandler
                },
                {
                    "entity:CommunalHelper/MaxHelpingHand/DreamFlagSwitchGate", flagSwitchGateHandler
                },
                {
                    "entity:MaxHelpingHand/RegularJumpThru", jumpthru => {
                        // those are actually just ... well, regular jumpthrus.
                        jumpthru.Name = "jumpThru";
                    }
                },
                {
                    "entity:MaxHelpingHand/MultiRoomStrawberrySeed", strawberrySeed => {
                        // auto-attribute indices for seeds, and save them.
                        string berryName = strawberrySeed.Attr("strawberryName");
                        if (multiRoomStrawberrySeedsByName.ContainsKey(berryName)) {
                            if (strawberrySeed.AttrInt("index") < 0) {
                                strawberrySeed.SetAttr("index", multiRoomStrawberrySeedsByName[berryName].Count);
                            }
                            multiRoomStrawberrySeedsByName[berryName].Add(strawberrySeed);
                        } else {
                            if (strawberrySeed.AttrInt("index") < 0) {
                                strawberrySeed.SetAttr("index", 0);
                            }
                            multiRoomStrawberrySeedsByName[berryName] = new List<BinaryPacker.Element>() { strawberrySeed };
                        }

                        Logger.Log(LogLevel.Verbose, "MaxHelpingHand/MapDataProcessor", $"area {AreaKey.SID} / mode {AreaKey.Mode} / room {levelName} has multi-room seed attached to strawberry {berryName} that was assigned index {strawberrySeed.AttrInt("index")}");
                    }
                },
                {
                    "entity:MaxHelpingHand/MultiRoomStrawberry", strawberry => {
                        // save the strawberry IDs.
                        string berryName = strawberry.Attr("name");
                        multiRoomStrawberryIDsByName[berryName] = new EntityID(levelName, strawberry.AttrInt("id"));
                        multiRoomStrawberriesByName[berryName] = strawberry;

                        Logger.Log(LogLevel.Verbose, "MaxHelpingHand/MapDataProcessor", $"area {AreaKey.SID} / mode {AreaKey.Mode} / room {levelName} has multi-room strawberry {berryName} with entity ID {strawberry.AttrInt("id")}");
                    }
                },
                {
                    "entity:MaxHelpingHand/SecretBerry", berry => {
                        if (berry.AttrBool("countTowardsTotal")) {
                            MapData.DetectedStrawberries++; // this is useful for file select slot berry count and print_counts.
                            DetectedSecretBerries++; // this will be picked up by a hook in SecretBerry.

                            Logger.Log(LogLevel.Verbose, "MaxHelpingHand/MapDataProcessor", $"area {AreaKey.SID} / mode {AreaKey.Mode} / room {levelName} has a secret berry that counts towards the total");
                        }
                    }
                },
                {
                    "entity:MaxHelpingHand/ShatterFlagSwitchGate", gate => {
                        // turn into a flag switch gate with isShatter = true.
                        gate.Name = "MaxHelpingHand/FlagSwitchGate";
                        gate.SetAttr("isShatter", true);

                        Logger.Log(LogLevel.Verbose, "MaxHelpingHand/MapDataProcessor", $"area {AreaKey.SID} / mode {AreaKey.Mode} / room {levelName} has a shatter flag switch gate that was turned into a regular flag switch gate");

                        flagSwitchGateHandler(gate);
                    }
                },
                {
                    "entity:MaxHelpingHand/MultiNodeBumper", bumper => {
                        // multi-node bumpers should never emit sound.
                        bumper.SetAttr("emitSound", false);

                        Logger.Log(LogLevel.Verbose, "MaxHelpingHand/MapDataProcessor", $"area {AreaKey.SID} / mode {AreaKey.Mode} / room {levelName} has a multi-node bumper that was made silent");
                    }
                },
                {
                    "entity:templeEye", eye => {
                        if (eye.AttrBool("followMadeline")) {
                            // this is a Temple Eye Tracking Madeline, except it used to be made with a custom attribute instead of a custom entity.
                            // so, convert it now!
                            eye.Name = "MaxHelpingHand/TempleEyeTrackingMadeline";

                            Logger.Log(LogLevel.Verbose, "MaxHelpingHand/MapDataProcessor", $"area {AreaKey.SID} / mode {AreaKey.Mode} / room {levelName} has a legacy temple eye from TempleMod, it was turned into a TempleEyeTrackingMadeline");
                        }
                    }
                }
            };
        }

        public override void Reset() {
            Logger.Log(LogLevel.Verbose, "MaxHelpingHand/MapDataProcessor", $"Resetting map data processor for {AreaKey.SID} / {AreaKey.Mode}!");

            if (!FlagTouchSwitches.ContainsKey(AreaKey.SID)) {
                // create an entry for the current map SID.
                FlagTouchSwitches[AreaKey.SID] = new List<Dictionary<KeyValuePair<string, bool>, List<EntityID>>>();
            }
            while (FlagTouchSwitches[AreaKey.SID].Count <= (int) AreaKey.Mode) {
                // fill out the empty space before the current map MODE with empty dictionaries.
                FlagTouchSwitches[AreaKey.SID].Add(new Dictionary<KeyValuePair<string, bool>, List<EntityID>>());
            }

            // reset the dictionary for the current map and mode.
            FlagTouchSwitches[AreaKey.SID][(int) AreaKey.Mode] = new Dictionary<KeyValuePair<string, bool>, List<EntityID>>();


            if (!FlagSwitchGates.ContainsKey(AreaKey.SID)) {
                // create an entry for the current map SID.
                FlagSwitchGates[AreaKey.SID] = new List<Dictionary<string, Dictionary<EntityID, bool>>>();
            }
            while (FlagSwitchGates[AreaKey.SID].Count <= (int) AreaKey.Mode) {
                // fill out the empty space before the current map MODE with empty dictionaries.
                FlagSwitchGates[AreaKey.SID].Add(new Dictionary<string, Dictionary<EntityID, bool>>());
            }

            // reset the dictionary for the current map and mode.
            FlagSwitchGates[AreaKey.SID][(int) AreaKey.Mode] = new Dictionary<string, Dictionary<EntityID, bool>>();
        }

        public override void End() {
            Logger.Log(LogLevel.Verbose, "MaxHelpingHand/MapDataProcessor", $"Finishing map data processor for {AreaKey.SID} / {AreaKey.Mode}!");

            foreach (string strawberryName in multiRoomStrawberrySeedsByName.Keys) {
                if (!multiRoomStrawberryIDsByName.ContainsKey(strawberryName)) {
                    Logger.Log(LogLevel.Warn, "MaxHelpingHandMapDataProcessor", $"Multi-room strawberry seeds with name {strawberryName} didn't match any multi-room strawberry");
                } else {
                    // give the strawberry ID to the seeds.
                    EntityID strawberryID = multiRoomStrawberryIDsByName[strawberryName];
                    foreach (BinaryPacker.Element strawberrySeed in multiRoomStrawberrySeedsByName[strawberryName]) {
                        strawberrySeed.SetAttr("berryLevel", strawberryID.Level);
                        strawberrySeed.SetAttr("berryID", strawberryID.ID);
                        strawberrySeed.SetAttr("seedCount", multiRoomStrawberrySeedsByName[strawberryName].Count);
                    }

                    // and give the expected seed count to the strawberry.
                    multiRoomStrawberriesByName[strawberryName].SetAttr("seedCount", multiRoomStrawberrySeedsByName[strawberryName].Count);
                }
            }

            multiRoomStrawberrySeedsByName.Clear();
            multiRoomStrawberryIDsByName.Clear();
            multiRoomStrawberriesByName.Clear();
            levelName = null;
        }
    }
}
