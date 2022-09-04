local effect = {}

effect.name = "MaxHelpingHand/CustomStars"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    spriteDirectory = {
        options = { "bgs/02/stars", "bgs/MaxHelpingHand/graystars" }
    },
    tint = {
        fieldType = "color"
    }
}

effect.defaultData = {
    spriteDirectory = "bgs/02/stars",
    tint = "ffffff",
    starCount = "",
    wrapHeight = 180.0,
    starAlpha = "",
    bgAlpha = 1.0,
    fadex = "",
    fadey = ""
}

return effect
