local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/ActivateTimedTouchSwitchesTimerTrigger"
trigger.triggerText = generateTriggerName
trigger.associatedMods = { "MaxHelpingHand", "OutbackHelper" }
trigger.placements = {
    name = "trigger"
}

return trigger