local glassBlockAreaController = {}

glassBlockAreaController.name = "MaxHelpingHand/CustomizableGlassBlockAreaController"
glassBlockAreaController.fillColor = {0.4, 0.4, 1.0, 0.4}
glassBlockAreaController.borderColor = {0.4, 0.4, 1.0, 1.0}
glassBlockAreaController.placements = {
    name = "field",
    data = {
        width = 8,
        height = 8,
        starColors = "ff7777,77ff77,7777ff,ff77ff,77ffff,ffff77",
        bgColor = "302040",
        wavy = false
    }
}

glassBlockAreaController.fieldInformation = {
    bgColor = {
        fieldType = "color"
    },
    starColors = {
        fieldType = "list",
        minimumElements = 1,
        elementDefault = "ffffff",
        elementOptions = {
            fieldType = "color",
            allowXNAColors = false
        }
    }
}

return glassBlockAreaController
