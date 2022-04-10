local colorController = {}

colorController.name = "MaxHelpingHand/SeekerBarrierColorController"
colorController.depth = 0
colorController.texture = "@Internal@/northern_lights"
colorController.placements = {
    name = "controller",
    data = {
        color = "FFFFFF",
        particleColor = "FFFFFF",
        transparency = 0.15,
        particleTransparency = 0.5,
        persistent = false,
        particleDirection = 0.0,
        depth = "",
        wavy = true
    }
}

colorController.fieldOrder = {"x", "y", "color", "transparency", "particleColor", "particleTransparency"}

colorController.fieldInformation = {
    color = {
        fieldType = "color"
    },
    particleColor = {
        fieldType = "color"
    }
}

return colorController
