local spikeHelper = require("helpers.spikes")

local spikeVariants = {
    "default",
    "outline",
    "cliffside",
    "reflection"
}

local spikeUp = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesUp", "up", false, false, spikeVariants)
local spikeDown = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesDown", "down", false, false, spikeVariants)
local spikeLeft = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesLeft", "left", false, false, spikeVariants)
local spikeRight = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesRight", "right", false, false, spikeVariants)

for key, value in ipairs({ spikeUp, spikeDown, spikeLeft, spikeRight }) do
    for placement in ipairs(value) do
        value.data.behindMoveBlocks = false
        value.data.triggerIfSameDirection = false
    end
end

return {
    spikeUp,
    spikeDown,
    spikeLeft,
    spikeRight
}
