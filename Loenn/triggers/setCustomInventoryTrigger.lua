local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/SetCustomInventoryTrigger"
trigger.triggerText = generateTriggerName
trigger.placements = {
    {
        name = "normal",
        data = {
            width = 8,
            height = 8,
            dashes = 1,
            backpack = true,
            dreamDash = false,
            groundRefills = true,
        },
    },
}
trigger.fieldInformation = {
    dashes = {
        fieldType = "integer",
    },
}

return trigger
