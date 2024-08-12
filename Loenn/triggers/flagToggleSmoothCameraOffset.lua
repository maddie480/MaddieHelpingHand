local enums = require("consts.celeste_enums")
local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/FlagToggleSmoothCameraOffsetTrigger"
trigger.category = "camera"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        offsetXFrom = 0.0,
        offsetXTo = 0.0,
        offsetYFrom = 0.0,
        offsetYTo = 0.0,
        positionMode = "NoEffect",
        onlyOnce = false,
        xOnly = false,
        yOnly = false,
        flag = "flag_toggle_smooth_camera_offset",
        inverted = false
    }
}

trigger.fieldInformation = {
    positionMode = {
        options = enums.trigger_position_modes,
        editable = false
    }
}

return trigger