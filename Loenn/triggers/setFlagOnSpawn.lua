local trigger = {}

trigger.name = "MaxHelpingHand/SetFlagOnSpawnTrigger"
trigger.placements = {
    name = "trigger",
    data = {
        flags = "flag_name",
        enable = false,
        ifFlag = ""
    }
}

return trigger