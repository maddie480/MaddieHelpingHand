local controller = {}

controller.name = "MaxHelpingHand/Pico8FlagController"
controller.depth = 0
controller.texture = "ahorn/MaxHelpingHand/pico_8_controller"
controller.placements = {
    {
        name = "complete",
        data = {
            flagOnComplete = "flag_name"
        }
    },
    {
        name = "berry",
        data = {
            flagOnBerryCount = "flag_name",
            berryCount = 18,
        }
    }
}

controller.fieldInformation = {
    berryCount = {
        fieldType = "integer"
    }
}

return controller
