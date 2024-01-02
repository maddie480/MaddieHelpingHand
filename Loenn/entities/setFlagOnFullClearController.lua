local controller = {}

controller.name = "MaxHelpingHand/SetFlagOnFullClearController"
controller.depth = 0
controller.texture = "ahorn/MaxHelpingHand/set_flag_on_spawn"
controller.placements = {
    name = "controller",
    data = {
        flag = "flag_name"
    }
}

return controller
