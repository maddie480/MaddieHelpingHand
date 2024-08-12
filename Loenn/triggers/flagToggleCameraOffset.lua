local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/FlagToggleCameraOffsetTrigger"
trigger.category = "camera"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        cameraX = 0.0,
        cameraY = 0.0,
        flag = "flag_toggle_camera_offset",
        inverted = false
    }
}

return trigger