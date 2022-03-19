local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableSprite = require("structs.drawable_sprite")

local switchGate = {}

switchGate.name = "MaxHelpingHand/MultiNodeFlagSwitchGate"
switchGate.depth = 0
switchGate.nodeLimits = {1, -1}
switchGate.nodeLineRenderType = "line"
switchGate.minimumSize = {16, 16}
switchGate.placements = {}

local textures = {
    "block", "mirror", "temple", "stars"
}
local bundledIcons = {
    "vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged"
}
local easeTypes = {
    "Linear", "SineIn", "SineOut", "SineInOut", "QuadIn", "QuadOut", "QuadInOut", "CubeIn", "CubeOut", "CubeInOut", "QuintIn", "QuintOut", "QuintInOut", "BackIn", "BackOut", "BackInOut", "ExpoIn", "ExpoOut", "ExpoInOut", "BigBackIn", "BigBackOut", "BigBackInOut", "ElasticIn", "ElasticOut", "ElasticInOut", "BounceIn", "BounceOut", "BounceInOut"
}

for i, texture in ipairs(textures) do
    switchGate.placements[i] = {
        name = texture,
        data = {
            width = 16,
            height = 16,
            inactiveColor = "5FCDE4",
            activeColor = "FFFFFF",
            finishColor = "F141DF",
            flags = "switch1,switch2",
            shakeTime = 0.5,
            moveTime = 2.0,
            easing = "CubeOut",
            sprite = texture,
            icon = "vanilla",
            resetFlags = true,
            canReturn = true,
            progressionMode = false,
            persistent = true,
            pauseTimes = "",
            doNotSkipNodes = false,
            smoke = true
        }
    }
end

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
    },
    easing = {
        options = easeTypes,
        editable = false
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
