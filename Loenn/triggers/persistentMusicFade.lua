local enums = require("consts.celeste_enums")
local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/PersistentMusicFadeTrigger"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        direction = "leftToRight",
        fadeA = 0.0,
        fadeB = 1.0,
        parameter = ""
    }
}

trigger.fieldInformation = {
    direction = {
        options = enums.music_fade_trigger_directions,
        editable = false
    }
}

return trigger