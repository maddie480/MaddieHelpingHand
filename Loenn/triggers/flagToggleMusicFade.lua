local enums = require("consts.celeste_enums")
local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local musicFade = {}

musicFade.name = "MaxHelpingHand/FlagToggleMusicFadeTrigger"
musicFade.triggerText = generateTriggerName
musicFade.fieldInformation = {
    direction = {
        options = enums.music_fade_trigger_directions,
        editable = false
    }
}
musicFade.placements = {
    name = "music_fade",
    data = {
        direction = "leftToRight",
        fadeA = 0.0,
        fadeB = 1.0,
        parameter = "",
        flag = "flag_toggle_music_fade",
        inverted = false
    }
}

musicFade.fieldInformation = {
    direction = {
        options = enums.music_fade_trigger_directions,
        editable = false
    }
}

return musicFade
