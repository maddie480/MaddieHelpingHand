local effect = {}

effect.name = "MaxHelpingHand/AllSideTentacles"
effect.canBackground = true
effect.canForeground = true

effect.fieldInformation = {
    side = {
        options = { "Top", "Bottom", "Left", "Right" },
        editable = false
    },
    color = {
        fieldType = "color"
    }
}

effect.defaultData = {
    side = "Left",
    color = "FFFFFF",
    offset = 0.0
}

return effect
