local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/RainbowSpinnerColorTrigger"
trigger.category = "visual"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        colors = "89E5AE,88E0E0,87A9DD,9887DB,D088E2",
        gradientSize = 280.0,
        loopColors = false,
        centerX = 0.0,
        centerY = 0.0,
        gradientSpeed = 50.0,
        persistent = false
    }
}
trigger.fieldInformation = {
    colors = {
        fieldType = "list",
        minimumElements = 1,
        elementDefault = "ffffff",
        elementOptions = {
            fieldType = "color",
            allowXNAColors = false
        }
    }
}

return trigger