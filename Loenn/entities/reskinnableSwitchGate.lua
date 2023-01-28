local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")
local enums = require("consts.celeste_enums")
local utils = require("utils")

local switchGate = {}

local textures = {
    "block", "mirror", "temple", "stars"
}
local textureOptions = {}

for _, texture in ipairs(textures) do
    textureOptions[utils.titleCase(texture)] = texture
end

switchGate.name = "MaxHelpingHand/ReskinnableSwitchGate"
switchGate.depth = 0
switchGate.nodeLimits = {1, 1}
switchGate.nodeLineRenderType = "line"
switchGate.minimumSize = {16, 16}
switchGate.fieldInformation = {
    sprite = {
        options = textureOptions
    },
    inactiveColor = {
        fieldType = "color"
    },
    activeColor = {
        fieldType = "color"
    },
    finishColor = {
        fieldType = "color"
    },
    icon = {
        options = {
            "objects/switchgate/icon",
            "objects/MaxHelpingHand/flagSwitchGate/tall/icon",
            "objects/MaxHelpingHand/flagSwitchGate/triangle/icon",
            "objects/MaxHelpingHand/flagSwitchGate/circle/icon",
            "objects/MaxHelpingHand/flagSwitchGate/diamond/icon",
            "objects/MaxHelpingHand/flagSwitchGate/double/icon",
            "objects/MaxHelpingHand/flagSwitchGate/heart/icon",
            "objects/MaxHelpingHand/flagSwitchGate/square/icon",
            "objects/MaxHelpingHand/flagSwitchGate/wide/icon",
            "objects/MaxHelpingHand/flagSwitchGate/winged/icon"
        }
    }
}
switchGate.placements = {}

for i, texture in ipairs(textures) do
    switchGate.placements[i] = {
        name = texture,
        data = {
            width = 16,
            height = 16,
            sprite = texture,
            persistent = false,
            icon = "objects/switchgate/icon",
            inactiveColor = "5fcde4",
            activeColor = "ffffff",
            finishColor = "f141df"
        }
    }
end

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local frameTexture = "objects/switchgate/%s"

function switchGate.sprite(room, entity)
    local middleTexture = entity.icon .. "00"

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local blockSprite = entity.sprite or "block"
    local frame = string.format(frameTexture, blockSprite)

    local ninePatch = drawableNinePatch.fromTexture(frame, ninePatchOptions, x, y, width, height)
    local middleSprite = drawableSprite.fromTexture(middleTexture, entity)
    local sprites = ninePatch:getDrawableSprite()

    middleSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, middleSprite)

    return sprites
end

function switchGate.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y
    local width, height = entity.width or 24, entity.height or 24

    return utils.rectangle(x, y, width, height), {utils.rectangle(nodeX, nodeY, width, height)}
end

return switchGate
