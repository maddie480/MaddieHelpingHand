local trigger = {}

trigger.name = "MaxHelpingHand/SpeedBasedMusicParamTrigger"
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

return trigger