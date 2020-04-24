using System;
using System.Collections.Generic;

namespace Celeste.Mod.MaxHelpingHand {
    class MaxHelpingHandMapDataProcessor : EverestMapDataProcessor {

        // the structure here is: FlagTouchSwitches[AreaID][ModeID][flagName] = list of entity ids for flag touch switches / flag switch gates in this group on this map.
        public static List<List<Dictionary<string, List<EntityID>>>> FlagTouchSwitches = new List<List<Dictionary<string, List<EntityID>>>>();
        public static List<List<Dictionary<string, Dictionary<EntityID, bool>>>> FlagSwitchGates = new List<List<Dictionary<string, Dictionary<EntityID, bool>>>>();
        private string levelName;

        public override Dictionary<string, Action<BinaryPacker.Element>> Init() {
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
                    "entity:MaxHelpingHand/FlagTouchSwitch", flagTouchSwitch => {
                        string flag = flagTouchSwitch.Attr("flag");
                        Dictionary<string, List<EntityID>> allTouchSwitchesInMap = FlagTouchSwitches[AreaKey.ID][(int) AreaKey.Mode];

                        // if no dictionary entry exists for this flag, create one. otherwise, get it.
                        List<EntityID> entityIDs;
                        if (!allTouchSwitchesInMap.ContainsKey(flag)) {
                            entityIDs = new List<EntityID>();
                            allTouchSwitchesInMap[flag] = entityIDs;
                        } else {
                            entityIDs = allTouchSwitchesInMap[flag];
                        }

                        // add this flag touch switch to the dictionary.
                        entityIDs.Add(new EntityID(levelName, flagTouchSwitch.AttrInt("id")));
                    }
                },
                {
                    "entity:MaxHelpingHand/FlagSwitchGate", flagSwitchGate => {
                        string flag = flagSwitchGate.Attr("flag");
                        Dictionary<string, Dictionary<EntityID, bool>> allSwitchGatesInMap = FlagSwitchGates[AreaKey.ID][(int) AreaKey.Mode];

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
                    }
                }
            };
        }

        public override void Reset() {
            while (FlagTouchSwitches.Count <= AreaKey.ID) {
                // fill out the empty space before the current map with empty dictionaries. 
                FlagTouchSwitches.Add(new List<Dictionary<string, List<EntityID>>>());
            }
            while (FlagTouchSwitches[AreaKey.ID].Count <= (int) AreaKey.Mode) {
                // fill out the empty space before the current map MODE with empty dictionaries. 
                FlagTouchSwitches[AreaKey.ID].Add(new Dictionary<string, List<EntityID>>());
            }

            // reset the dictionary for the current map and mode.
            FlagTouchSwitches[AreaKey.ID][(int) AreaKey.Mode] = new Dictionary<string, List<EntityID>>();


            while (FlagSwitchGates.Count <= AreaKey.ID) {
                // fill out the empty space before the current map with empty dictionaries. 
                FlagSwitchGates.Add(new List<Dictionary<string, Dictionary<EntityID, bool>>>());
            }
            while (FlagSwitchGates[AreaKey.ID].Count <= (int) AreaKey.Mode) {
                // fill out the empty space before the current map MODE with empty dictionaries. 
                FlagSwitchGates[AreaKey.ID].Add(new Dictionary<string, Dictionary<EntityID, bool>>());
            }

            // reset the dictionary for the current map and mode.
            FlagSwitchGates[AreaKey.ID][(int) AreaKey.Mode] = new Dictionary<string, Dictionary<EntityID, bool>>();
        }

        public override void End() {
            // nothing required.
        }
    }
}