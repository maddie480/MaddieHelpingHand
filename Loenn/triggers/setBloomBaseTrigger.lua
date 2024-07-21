local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/SetBloomBaseTrigger"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        value = 0.0
    }
}

return trigger