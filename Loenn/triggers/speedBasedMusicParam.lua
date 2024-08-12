local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/SpeedBasedMusicParamTrigger"
trigger.category = "audio"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        paramName = "fade",
        minSpeed = 0.0,
        maxSpeed = 90.0,
        minParamValue = 0.0,
        maxParamValue = 1.0,
        activate = true
    }
}

trigger.editingOrder = {"x", "y", "width", "height", "minSpeed", "maxSpeed", "minParamValue", "maxParamValue"}

return trigger