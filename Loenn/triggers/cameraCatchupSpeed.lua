local trigger = {}

trigger.name = "MaxHelpingHand/CameraCatchupSpeedTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        catchupSpeed = 1.0,
        revertOnLeave = true
    }
}

return trigger