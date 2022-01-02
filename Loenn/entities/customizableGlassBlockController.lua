local customizableGlassBlockController = {}

customizableGlassBlockController.name = "MaxHelpingHand/CustomizableGlassBlockController"
customizableGlassBlockController.depth = 0
customizableGlassBlockController.texture = "@Internal@/northern_lights"
customizableGlassBlockController.placements = {
    name = "controller",
    data = {
        starColors = "ff7777,77ff77,7777ff,ff77ff,77ffff,ffff77",
        bgColor = "302040",
        wavy = false,
        persistent = false
    }
}

customizableGlassBlockController.fieldInformation = {
    bgColor = {
        fieldType = "color"
    }
}

return customizableGlassBlockController
