local controller = {}

controller.name = "MaxHelpingHand/SetFlagOnActionController"
controller.depth = 0
controller.texture = "ahorn/MaxHelpingHand/set_flag_on_action"
controller.placements = {
    name = "controller",
    data = {
        action = "Climb",
        flag = "flag_name",
        inverted = false
    }
}

controller.fieldInformation = {
    action = {
        options = { "OnGround", "InAir", "Climb", "Dash", "Swim" },
        editable = false
    }
}

return controller
