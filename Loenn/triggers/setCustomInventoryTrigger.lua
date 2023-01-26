local trigger = {}

trigger.name = "MaxHelpingHand/SetCustomInventoryTrigger"
trigger.placements = {
    {
        name = "normal",
        data = {
            width = 8,
            height = 8,
            dashes = 1,
            backpack = true,
            dreamDash = false,
            groundRefills = true,
        },
    },
}
trigger.fieldInformation = {
    dashes = {
        fieldType = "integer",
    },
}

return trigger
