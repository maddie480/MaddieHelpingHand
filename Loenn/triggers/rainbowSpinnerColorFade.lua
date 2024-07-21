local enums = require("consts.celeste_enums")
local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/RainbowSpinnerColorFadeTrigger"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        colorsA = "89E5AE,88E0E0,87A9DD,9887DB,D088E2",
        gradientSizeA = 280.0,
        loopColorsA = false,
        centerXA = 0.0,
        centerYA = 0.0,
        gradientSpeedA = 50.0,
        colorsB = "89E5AE,88E0E0,87A9DD,9887DB,D088E2",
        gradientSizeB = 280.0,
        loopColorsB = false,
        centerXB = 0.0,
        centerYB = 0.0,
        gradientSpeedB = 50.0,
        direction = "NoEffect",
        persistent = false
    }
}

trigger.fieldInformation = {
    direction = {
        options = enums.trigger_position_modes,
        editable = false
    }
}

return trigger