local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/OneWayCameraTrigger"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        left = true,
        right = true,
        up = true,
        down = true,
        flag = "",
        blockPlayer = false
    }
}

return trigger