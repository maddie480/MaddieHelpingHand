local colorController = {}

colorController.name = "MaxHelpingHand/RainbowSpinnerColorController"
colorController.depth = 0
colorController.texture = "@Internal@/northern_lights"
colorController.placements = {
    name = "controller",
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

return colorController
