local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/MadelineSilhouetteTrigger"
trigger.category = "visual"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        enable = true
    }
}

return trigger