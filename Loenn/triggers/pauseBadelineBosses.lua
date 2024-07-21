local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/PauseBadelineBossesTrigger"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger"
}

return trigger