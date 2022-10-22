local spikeHelper = require("helpers.spikes")

local spikeVariants = {
    "default",
    "outline",
    "cliffside",
    "reflection",
    "dust"
}

local spikeUp = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesUp", "up", false, true, spikeVariants)
local spikeDown = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesDown", "down", false, true, spikeVariants)
local spikeLeft = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesLeft", "left", false, true, spikeVariants)
local spikeRight = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesRight", "right", false, true, spikeVariants)

local allSpikes = { spikeUp, spikeDown, spikeLeft, spikeRight }

local regularSpikes = {
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesUp", "up", false, true),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesDown", "down", false, true),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesLeft", "left", false, true),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesRight", "right", false, true)
}

local dustSpikes = {
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesUp", "up", true),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesDown", "down", true),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesLeft", "left", true),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesRight", "right", true)
}

-- pick between the dust handler and the regular handler depending on the entity type
for i=1,4,1 do
    local spike = allSpikes[i]
    local regularSpike = regularSpikes[i]
    local dustSpike = dustSpikes[i]

    function spike.sprite(room, entity)
        if entity.type == "dust" then
            return dustSpike.sprite(room, entity)
        else
            return regularSpike.sprite(room, entity)
        end
    end

    function spike.selection(room, entity)
        if entity.type == "dust" then
            return dustSpike.selection(room, entity)
        else
            return regularSpike.selection(room, entity)
        end
    end
end


for key, value in ipairs(allSpikes) do
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
