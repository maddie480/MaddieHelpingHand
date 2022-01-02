local trigger = {}

trigger.name = "MaxHelpingHand/FlagToggleCameraTargetTrigger"
trigger.nodeLimits = {1, 1}
trigger.placements = {
    name = "trigger",
    data = {
        lerpStrength = 0.0,
        positionMode = "NoEffect",
        xOnly = false,
        yOnly = false,
        deleteFlag = "",
        flag = "flag_toggle_camera_target",
        inverted = false
    }
}

return trigger