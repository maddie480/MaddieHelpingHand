local spikeHelper = require("helpers.spikes")

local spikeUp = spikeHelper.createEntityHandler("MaxHelpingHand/CoreModeSpikesUp", "up")
local spikeDown = spikeHelper.createEntityHandler("MaxHelpingHand/CoreModeSpikesDown", "down")
local spikeLeft = spikeHelper.createEntityHandler("MaxHelpingHand/CoreModeSpikesLeft", "left")
local spikeRight = spikeHelper.createEntityHandler("MaxHelpingHand/CoreModeSpikesRight", "right")

spikeUp.direction = "up"
spikeDown.direction = "down"
spikeLeft.direction = "left"
spikeRight.direction = "right"

local spikes = {
    spikeUp,
    spikeDown,
    spikeLeft,
    spikeRight
}

local function getSpikePlacements(direction)
    local placements = {}
    local horizontal = direction == "left" or direction == "right"
    local lengthKey = horizontal and "height" or "width"

    local placement = {
        name = "spike",
        data = {
            hotType = "MaxHelpingHand/heatspike",
            coldType = "cliffside",
        }
    }

    placement.data[lengthKey] = 8

    return { placement }
end

-- Lönn spike helper copypaste begin
local drawableSprite = require("structs.drawable_sprite")

local spikeTexture = "danger/spikes/%s_%s00"

local spikeOffsets = {
    up = {0, 1},
    down = {0, -1},
    right = {-1, 0},
    left = {1, -1}
}

local spikeJustifications = {
    up = {0.0, 1.0},
    down = {0.0, 0.0},
    right = {0.0, 0.0},
    left = {1.0, 0.0}
}

local function getDirectionJustification(direction)
    return unpack(spikeJustifications[direction] or {0, 0})
end

local function getDirectionOffset(direction)
    return unpack(spikeOffsets[direction] or {0, 0})
end

local function getSpikeSpritesFromTexture(entity, direction, variant, texture)
    step = 8

    local horizontal = direction == "left" or direction == "right"
    local justificationX, justificationY = getDirectionJustification(direction)
    local offsetX, offsetY = getDirectionOffset(direction)
    local rotation = 0
    local length = horizontal and (entity.height or step) or (entity.width or step)
    local positionOffsetKey = horizontal and "y" or "x"

    local position = {
        x = entity.x,
        y = entity.y
    }

    local sprites = {}

    for i = 0, length - 1, step do
        -- Tentacles overlap instead of "overflowing"
        if i == length - step / 2 then
            position[positionOffsetKey] -= step / 2
        end

        local sprite = drawableSprite.fromTexture(texture, position)

        sprite.depth = spikeDepth
        sprite.rotation = rotation
        sprite:setJustification(justificationX, justificationY)
        sprite:addPosition(offsetX, offsetY)

        table.insert(sprites, sprite)

        position[positionOffsetKey] += step
    end

    return sprites
end

local function getNormalSpikeSprites(entity, direction)
    local variant = entity.hotType or "default"
    local texture = string.format(spikeTexture, variant, direction)

    return getSpikeSpritesFromTexture(entity, direction, variant, texture)
end
-- Lönn spike helper copypaste end

-- TODO use right spike textures
for _, spike in ipairs(spikes) do
    spike.placements = getSpikePlacements(spike.direction)

    local direction = spike.direction
    function spike.sprite(room, entity)
        return getNormalSpikeSprites(entity, direction)
    end
end

return spikes
