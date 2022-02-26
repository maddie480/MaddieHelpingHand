local spikeHelper = require("helpers.spikes")

local spikeVariants = {
    "default",
    "outline",
    "cliffside",
    "reflection",
    "dust" -- TODO actually render them
}

local spikeUp = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesUp", "up", false, true, spikeVariants)
local spikeDown = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesDown", "down", false, true, spikeVariants)
local spikeLeft = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesLeft", "left", false, true, spikeVariants)
local spikeRight = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesRight", "right", false, true, spikeVariants)

for key, value in ipairs({ spikeUp, spikeDown, spikeLeft, spikeRight }) do
    for key2, placement in ipairs(value.placements) do
        placement.data.behindMoveBlocks = false
        placement.data.triggerIfSameDirection = false
        placement.data.killIfSameDirection = false
    end
end

return {
    spikeUp,
    spikeDown,
    spikeLeft,
    spikeRight
}
