local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/FlagToggleCameraCatchupSpeedTrigger"
trigger.category = "camera"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        catchupSpeed = 1.0,
        revertOnLeave = true,
        flag = "flag_toggle_camera_catchup_speed",
        inverted = false
    }
}

return trigger