local controller = {}

controller.name = "MaxHelpingHand/SetFlagOnSpawnController"
controller.depth = 0
controller.texture = "ahorn/MaxHelpingHand/set_flag_on_spawn"
controller.placements = {
    name = "controller",
    data = {
        flag = "flag_name",
        enable = false,
        onlyOnRespawn = false,
        ifFlag = ""
    }
}

return controller
