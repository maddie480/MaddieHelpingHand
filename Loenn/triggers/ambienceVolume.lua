local enums = require("consts.celeste_enums")

local trigger = {}

trigger.name = "MaxHelpingHand/AmbienceVolumeTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        from = 0.0,
        to = 0.0,
        direction = "NoEffect"
    }
}

trigger.fieldInformation = {
    direction = {
        options = enums.trigger_position_modes,
        editable = false
    }
}

return trigger