local enums = require("consts.celeste_enums")
local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/AllBlackholesStrengthTrigger"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        strength = "Mild"
    }
}
trigger.fieldInformation = {
    strength = {
        options = enums.black_hole_trigger_strengths,
        editable = false
    }
}

return trigger