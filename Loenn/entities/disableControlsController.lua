local controller = {}

controller.name = "MaxHelpingHand/DisableControlsController"
controller.depth = 0
controller.texture = "ahorn/MaxHelpingHand/disable_controls"
controller.placements = {
    name = "controller",
    data = {
        up = false,
        down = false,
        left = false,
        right = false,
        jump = false,
        dash = false,
        grab = false,
        onlyIfFlag = ""
    }
}

return controller
