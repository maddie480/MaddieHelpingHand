local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

local colorGrades = {
    "none", "oldsite", "panicattack", "templevoid", "reflection", "credits", "cold", "hot", "feelingdown", "golden"
}

trigger.name = "MaxHelpingHand/ColorGradeFadeTrigger"
trigger.category = "visual"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        colorGradeA = "none",
        colorGradeB = "none",
        direction = "LeftToRight",
        evenDuringReflectionFall = false
    }
}

trigger.fieldInformation = {
    colorGradeA = {
        options = colorGrades
    },
    colorGradeB = {
        options = colorGrades
    },
    direction = {
        options = { "LeftToRight", "TopToBottom" },
        editable = false
    }
}

return trigger