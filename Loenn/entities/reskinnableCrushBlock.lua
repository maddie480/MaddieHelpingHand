local drawableNinePatch = require("structs.drawable_nine_patch")
local drawableRectangle = require("structs.drawable_rectangle")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local kevin = {}

kevin.name = "MaxHelpingHand/ReskinnableCrushBlock"
kevin.depth = 0
kevin.minimumSize = {24, 24}
kevin.placements = {}

local axesOptions = {
    "both", "vertical", "horizontal"
}

for _, axes in ipairs(axesOptions) do
    table.insert(kevin.placements, {
        name = axes,
        data = {
            width = 24,
            height = 24,
            axes = axes,
            chillout = false,
            spriteDirectory = "objects/crushblock",
            fillColor = "62222b",
            crushParticleColor1 = "ff66e2",
            crushParticleColor2 = "68fcff",
            activateParticleColor1 = "5fcde4",
            activateParticleColor2 = "ffffff",
            soundDirectory = "event:/game/06_reflection"
        }
    })
end

kevin.fieldInformation = {
    fillColor = {
        fieldType = "color"
    },
    crushParticleColor1 = {
        fieldType = "color"
    },
    crushParticleColor2 = {
        fieldType = "color"
    },
    activateParticleColor1 = {
        fieldType = "color"
    },
    activateParticleColor2 = {
        fieldType = "color"
    },
    axes = {
        options = axesOptions,
        editable = false
    }
}

local frameTextures = {
    none = "/block00",
    horizontal = "/block01",
    vertical = "/block02",
    both = "/block03"
}

local ninePatchOptions = {
    mode = "border",
    borderMode = "repeat"
}

function kevin.sprite(room, entity)
    local success, r, g, b = utils.parseHexColor(entity.fillColor)

    local kevinColor = {98 / 255, 34 / 255, 43 / 255}
    if success then
        kevinColor = {r, g, b}
    end

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local axes = entity.axes or "both"
    local chillout = entity.chillout

    local giant = height >= 48 and width >= 48 and chillout
    local faceTexture = entity.spriteDirectory .. (giant and "/giant_block00" or "/idle_face")

    local frameTexture = entity.spriteDirectory .. (frameTextures[axes] or frameTextures["both"])
    local ninePatch = drawableNinePatch.fromTexture(frameTexture, ninePatchOptions, x, y, width, height)

    local rectangle = drawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, kevinColor)
    local faceSprite = drawableSprite.fromTexture(faceTexture, entity)

    faceSprite:addPosition(math.floor(width / 2), math.floor(height / 2))

    local sprites = ninePatch:getDrawableSprite()

    table.insert(sprites, 1, rectangle:getDrawableSprite())
    table.insert(sprites, 2, faceSprite)

    return sprites
end

return kevin
