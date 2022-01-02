local colorController = {}

colorController.name = "MaxHelpingHand/FlagRainbowSpinnerColorController"
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
        persistent = false,
        flag = "my_flag",
        colorsWithFlag = "89E5AE,88E0E0,87A9DD,9887DB,D088E2",
        gradientSizeWithFlag = 280.0,
        loopColorsWithFlag = false,
        centerXWithFlag = 0.0,
        centerYWithFlag = 0.0,
        gradientSpeedWithFlag = 50.0
    }
}

colorController.fieldOrder = {"x", "y", "centerX", "centerXWithFlag", "centerY", "centerYWithFlag", "colors", "colorsWithFlag", "gradientSize", "gradientSizeWithFlag", "gradientSpeed", "gradientSpeedWithFlag"}

return colorController
