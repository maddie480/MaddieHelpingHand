local effect = {}

effect.name = "MaxHelpingHand/NorthernLightsCustomColors"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    gradientColor1 = {
        fieldType = "color"
    },
    gradientColor2 = {
        fieldType = "color"
    },
    particleCount = {
        fieldType = "integer"
    },
    strandCount = {
        fieldType = "integer"
    }
}

effect.defaultData = {
    gradientColor1 = "020825",
    gradientColor2 = "170c2f",
    colors = "2de079,62f4f6,45bc2e,3856f0",
    displayBackground = true,
    particleCount = 50,
    strandCount = 3
}

return effect
