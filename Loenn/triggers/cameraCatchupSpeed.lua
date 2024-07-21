local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/CameraCatchupSpeedTrigger"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        catchupSpeed = 1.0,
        revertOnLeave = true
    }
}

return trigger