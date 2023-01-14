local trigger = {}

trigger.name = "MaxHelpingHand/ExtendedDialogCutsceneTrigger"

trigger.placements = {
    name = "trigger",
    data = {
        endLevel = false,
        onlyOnce = true,
        dialogId = "",
        deathCount = -1,
        font = ""
    }
}

trigger.fieldInformation = {
    deathCount = {
        fieldType = "integer"
    }
}

return trigger
