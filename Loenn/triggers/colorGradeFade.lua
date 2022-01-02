local trigger = {}

trigger.name = "MaxHelpingHand/ColorGradeFadeTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        colorGradeA = "none",
        colorGradeB = "none",
        direction = "LeftToRight",
        evenDuringReflectionFall = false
    }
}

return trigger