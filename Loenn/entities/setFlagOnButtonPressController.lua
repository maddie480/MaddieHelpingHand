local controller = {}

controller.name = "MaxHelpingHand/SetFlagOnButtonPressController"
controller.depth = 0
controller.texture = "ahorn/MaxHelpingHand/set_flag_on_button"
controller.placements = {
    name = "controller",
    data = {
        button = "Grab",
        flag = "flag_name",
        inverted = false,
        toggleMode = false,
        activationDelay = 0.0
    }
}

controller.fieldInformation = {
    button = {
        options = {
            "Jump",
            "Dash",
            "Grab",
            "Talk",
            "CrouchDash",
            "ESC",
            "Pause",
            "MenuLeft",
            "MenuRight",
            "MenuUp",
            "MenuDown",
            "MenuConfirm",
            "MenuCancel",
            "MenuJournal",
            "QuickRestart"
        },
        editable = false
    }
}

return controller
