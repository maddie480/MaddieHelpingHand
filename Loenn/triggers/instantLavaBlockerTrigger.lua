local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local lavaBlocker = {}

lavaBlocker.name = "MaxHelpingHand/InstantLavaBlockerTrigger"
lavaBlocker.triggerText = generateTriggerName
lavaBlocker.placements = {
    name = "lava_blocker",
    data = {
        canReenter = false
    }
}

return lavaBlocker
