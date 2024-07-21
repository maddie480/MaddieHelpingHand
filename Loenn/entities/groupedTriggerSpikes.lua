local spikeHelper = require("helpers.spikes")

local spikeVariants = {
    "default",
    "outline",
    "cliffside",
    "reflection",
    "dust"
}

local spikeOptions = {
    triggerSpike = false,
    originalTriggerSpike = true,
    variants = spikeVariants
}

local spikeUp = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesUp", "up", spikeOptions)
local spikeDown = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesDown", "down", spikeOptions)
local spikeLeft = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesLeft", "left", spikeOptions)
local spikeRight = spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesRight", "right", spikeOptions)

local allSpikes = { spikeUp, spikeDown, spikeLeft, spikeRight }

local regularSpikeOptions = {
    triggerSpike = false,
    originalTriggerSpike = true
}

local regularSpikes = {
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesUp", "up", regularSpikeOptions),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesDown", "down", regularSpikeOptions),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesLeft", "left", regularSpikeOptions),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesRight", "right", regularSpikeOptions)
}

local dustSpikeOptions = {
    triggerSpike = true
}

local dustSpikes = {
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesUp", "up", dustSpikeOptions),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesDown", "down", dustSpikeOptions),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesLeft", "left", dustSpikeOptions),
    spikeHelper.createEntityHandler("MaxHelpingHand/GroupedTriggerSpikesRight", "right", dustSpikeOptions)
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
