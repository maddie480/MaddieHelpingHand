local effect = {}

effect.name = "MaxHelpingHand/CustomPlanets"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    count = {
        fieldType = "integer"
    },
    color = {
        fieldType = "color"
    },
    directory = {
        options = {
            "MaxHelpingHand/customplanets/bigstars",
            "MaxHelpingHand/customplanets/smallstars",
            "MaxHelpingHand/customplanets/bigplanets",
            "MaxHelpingHand/customplanets/smallplanets",
            "MaxHelpingHand/customplanets/rainbowstars"
        }
    }
}

effect.defaultData = {
    count = 32,
    color = "FFFFFF",
    scrollx = 1.0,
    scrolly = 1.0,
    speedx = 0.0,
    speedy = 0.0,
    directory = "MaxHelpingHand/customplanets/bigstars",
    animationDelay = 0.1,
    fadex = "",
    fadey = ""
}

return effect
