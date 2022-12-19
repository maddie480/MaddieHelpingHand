local trigger = {}

trigger.name = "MaxHelpingHand/AutoSkipDialogCutsceneTrigger"

trigger.placements = {
    name = "trigger",
    data = {
        endLevel = false,
        onlyOnce = true,
        dialogId = "",
        deathCount = -1
    }
}

trigger.fieldInformation = {
    deathCount = {
        fieldType = "integer"
    }
}

return trigger
