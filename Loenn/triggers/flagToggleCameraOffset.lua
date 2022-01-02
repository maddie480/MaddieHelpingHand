local trigger = {}

trigger.name = "MaxHelpingHand/FlagToggleCameraOffsetTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        cameraX = 0.0,
        cameraY = 0.0,
        flag = "flag_toggle_camera_offset",
        inverted = false
    }
}

return trigger