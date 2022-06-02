local drawableNinePatch = require("structs.drawable_nine_patch")
local utils = require("utils")

local crumbleBlock = {}

local textures = {
    "default", "cliffside"
}

crumbleBlock.name = "MaxHelpingHand/CustomizableCrumblePlatform"
crumbleBlock.depth = 0
crumbleBlock.placements = {}

for _, texture in ipairs(textures) do
    table.insert(crumbleBlock.placements, {
        name = texture,
        data = {
            width = 8,
            texture = texture,
            oneUse = false,
            respawnDelay = 2.0,
            grouped = false,
            minCrumbleDurationOnTop = 0.2,
            maxCrumbleDurationOnTop = 0.6,
            crumbleDurationOnSide = 1.0,
            outlineTexture = "objects/crumbleBlock/outline",
            onlyEmitSoundForPlayer = false,
            fadeOutTint = "808080",
            attachStaticMovers = false,
            flag = "",
            setFlagOnPlayerContact = false,
            flagMode = "None",
            flagInverted = false
        }
    })
end

crumbleBlock.fieldOrder = {"x", "y", "width", "crumbleDurationOnSide", "minCrumbleDurationOnTop", "maxCrumbleDurationOnTop"}

crumbleBlock.fieldInformation = {
    fadeOutTint = {
        fieldType = "color"
    },
    texture = {
        options = textures
    },
    flagMode = {
        options = { "None", "UntilPlatformRespawn", "UntilDeathOrRoomChange", "Permanent" },
        editable = false
    }
}

local ninePatchOptions = {
    mode = "fill",
    fillMode = "repeat",
    border = 0
}

function crumbleBlock.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width = math.max(entity.width or 0, 8)

    local variant = entity.texture or "default"
    local texture = "objects/crumbleBlock/" .. variant
    local ninePatch = drawableNinePatch.fromTexture(texture, ninePatchOptions, x, y, width, 8)

    return ninePatch
end

function crumbleBlock.selection(room, entity)
    return utils.rectangle(entity.x or 0, entity.y or 0, math.max(entity.width or 0, 8), 8)
end

return crumbleBlock
