local enums = require("consts.celeste_enums")

local trigger = {}

trigger.name = "MaxHelpingHand/BloomBaseFadeTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        fadeA = 0.0,
        fadeB = 1.0,
        positionMode = "LeftToRight"
    }
}

trigger.fieldInformation = {
    positionMode = {
        options = enums.trigger_position_modes,
        editable = false
    }
}

return trigger
