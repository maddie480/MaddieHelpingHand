local areaController = {}

areaController.name = "MaxHelpingHand/RainbowSpinnerColorAreaController"
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
        gradientSpeed = 50.0
    }
}

return areaController
