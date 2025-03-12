local areaController = {}

areaController.name = "MaxHelpingHand/FlagRainbowSpinnerColorAreaController"
areaController.fillColor = {0.4, 0.4, 1.0, 0.4}
areaController.borderColor = {0.4, 0.4, 1.0, 1.0}
areaController.placements = {
    name = "field",
    data = {
        width = 8,
        height = 8,
        colors = "89E5AE,88E0E0,87A9DD,9887DB,D088E2",
        gradientSize = 280.0,
        loopColors = false,
        centerX = 0.0,
        centerY = 0.0,
        gradientSpeed = 50.0,
        flag = "my_flag",
        colorsWithFlag = "89E5AE,88E0E0,87A9DD,9887DB,D088E2",
        gradientSizeWithFlag = 280.0,
        loopColorsWithFlag = false,
        centerXWithFlag = 0.0,
        centerYWithFlag = 0.0,
        gradientSpeedWithFlag = 50.0
    }
}

areaController.fieldOrder = {"x", "y", "width", "height", "centerX", "centerXWithFlag", "centerY", "centerYWithFlag", "colors", "colorsWithFlag", "gradientSize", "gradientSizeWithFlag", "gradientSpeed", "gradientSpeedWithFlag"}

areaController.fieldInformation = {
    colors = {
        fieldType = "list",
        minimumElements = 1,
        elementDefault = "ffffff",
        elementOptions = {
            fieldType = "color",
            allowXNAColors = false
        }
    },
    colorsWithFlag = {
        fieldType = "list",
        minimumElements = 1,
        elementDefault = "ffffff",
        elementOptions = {
            fieldType = "color",
            allowXNAColors = false
        }
    }
}

return areaController
