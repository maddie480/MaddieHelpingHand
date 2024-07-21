local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/StrawberryCollectionField"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        delayBetweenBerries = true,
        includeGoldens = false
    }
}

return trigger