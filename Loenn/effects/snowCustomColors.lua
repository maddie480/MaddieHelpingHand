local effect = {}

effect.name = "MaxHelpingHand/SnowCustomColors"
effect.canBackground = true
effect.canForeground = true

effect.defaultData = {
    colors = "FF0000,00FF00,0000FF",
    speedMin = 40.0,
    speedMax = 100.0,
    alpha = 1.0,
    particleCount = 60,
    angle = 180.0,
    sineAmplitudeMult = 0.2,
    texturePath = "",
    scrollx = 1.0,
    scrolly = 1.0,
    fadex = "",
    fadey = ""
}

effect.fieldOrder = {
    "only", "exclude", "tag", "flag",
    "colors", "alpha", "particleCount", "notflag",
    "speedMin", "speedMax", "scrollx", "scrolly",
    "angle", "sineAmplitudeMult", "fadex", "fadey",
    "texturePath"
}

effect.fieldInformation = {
    colors = {
        fieldType = "list",
        minimumElements = 1,
        elementDefault = "ffffff",
        elementOptions = {
            fieldType = "color",
            allowXNAColors = false
        }
    },
    alpha = {
        maximumValue = 1.0,
        minimumValue = 0.0
    },
    particleCount = {
        fieldType = "integer",
        minimumValue = 1
    }
}

return effect
