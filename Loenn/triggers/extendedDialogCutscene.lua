local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/ExtendedDialogCutsceneTrigger"
trigger.triggerText = generateTriggerName

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
