local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local switchGate = {}

switchGate.name = "MaxHelpingHand/ShatterFlagSwitchGate"
switchGate.depth = 0
switchGate.minimumSize = {16, 16}
switchGate.associatedMods = { "MaxHelpingHand", "VortexHelper" }
switchGate.placements = {}

local textures = {
    "block", "mirror", "temple", "stars"
}
local bundledIcons = {
    "vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged", "cross", "drop", "hourglass", "split", "star", "triple"
}

for i, texture in ipairs(textures) do
    switchGate.placements[i] = {
        name = texture,
        data = {
            width = 16,
            height = 16,
            sprite = texture,
            persistent = false,
            flag = "flag_touch_switch",
            icon = "vanilla",
            inactiveColor = "5FCDE4",
            activeColor = "FFFFFF",
            finishColor = "F141DF",
            shakeTime = 0.5,
            finishedSound = "event:/game/general/touchswitch_gate_finish",
            debrisPath = "",
            shatterSound = true
        }
    }
end

switchGate.fieldOrder = {"x", "y", "width", "height", "flag", "inactiveColor", "activeColor", "finishColor", "finishedSound", "shakeTime"}

switchGate.fieldInformation = {
    inactiveColor = {
        fieldType = "color"
    },
    activeColor = {
        fieldType = "color"
    },
    finishColor = {
        fieldType = "color"
    },
    sprite = {
        options = textures
    },
    icon = {
        options = bundledIcons
    }
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local frameTexture = "objects/switchgate/%s"

function switchGate.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local blockSprite = entity.sprite or "block"
    local frame = string.format(frameTexture, blockSprite)

    iconResource = "objects/switchgate/icon00"
    if entity.icon ~= "vanilla" then
        iconResource = "objects/MaxHelpingHand/flagSwitchGate/" .. entity.icon .."/icon00"
    end

    local ninePatch = drawableNinePatch.fromTexture(frame, ninePatchOptions, x, y, width, height)
    local middleSprite = drawableSprite.fromTexture(iconResource, entity)
    local sprites = ninePatch:getDrawableSprite()

    middleSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, middleSprite)

    return sprites
end

return switchGate
