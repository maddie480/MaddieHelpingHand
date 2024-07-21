local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/CustomSandwichLavaSettingsTrigger"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        onlyOnce = false,
        direction = "CoreModeBased",
        speed = 20.0
    }
}

trigger.fieldInformation = {
    direction = {
        options = {
            ["Always Up"] = "AlwaysUp",
            ["Always Down"] = "AlwaysDown",
            ["Based on Core Mode"] = "CoreModeBased"
        },
        editable = false
    }
}

return trigger