local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/GradientDustTrigger"
trigger.category = "visual"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        gradientImage = "MaxHelpingHand/gradientdustbunnies/bluegreen",
        scrollSpeed = 50.0
    }
}

return trigger