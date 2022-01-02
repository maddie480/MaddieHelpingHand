local trigger = {}

trigger.name = "MaxHelpingHand/FlagToggleSmoothCameraOffsetTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        offsetXFrom = 0.0,
        offsetXTo = 0.0,
        offsetYFrom = 0.0,
        offsetYTo = 0.0,
        positionMode = "NoEffect",
        onlyOnce = false,
        xOnly = false,
        yOnly = false,
        flag = "flag_toggle_smooth_camera_offset",
        inverted = false
    }
}

return trigger