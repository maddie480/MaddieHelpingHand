local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local respawningBounceJellyfish = {}

respawningBounceJellyfish.name = "MaxHelpingHand/RespawningBounceJellyfish"
respawningBounceJellyfish.depth = -5
respawningBounceJellyfish.associatedMods = { "MaxHelpingHand", "BounceHelper" }
local respawningBounceJellyfishDashCounts = {
    0, 1, 2
}

respawningBounceJellyfish.fieldInformation = {
    baseDashCount = {
        options = respawningBounceJellyfishDashCounts,
        editable = false
    }
}
respawningBounceJellyfish.placements = {
    {
        name = "normal",
        data = {
            platform = true,
            soulBound = false,
            baseDashCount = 0,
            respawnTime = 2.0,
            spriteDirectory = "objects/MaxHelpingHand/glider"
        }
    },
    {
        name = "single",
        data = {
            platform = true,
            soulBound = false,
            baseDashCount = 1,
            respawnTime = 2.0,
            spriteDirectory = "objects/MaxHelpingHand/glider"
        }
    },
    {
        name = "double",
        data = {
            platform = true,
            soulBound = false,
            baseDashCount = 2,
            respawnTime = 2.0,
            spriteDirectory = "objects/MaxHelpingHand/glider"
        }
    }
}

function respawningBounceJellyfish.sprite(room, entity)
    local bubble = entity.platform
    local texture = entity.spriteDirectory .. "/idle0"

    if entity.bubble then
        local x, y = entity.x or 0, entity.y or 0
        local points = drawing.getSimpleCurve({x - 11, y - 1}, {x + 11, y - 1}, {x - 0, y - 6})
        local lineSprites = drawableLine.fromPoints(points):getDrawableSprite()
        local jellySprite = drawableSprite.fromTexture(texture, entity)
        jellySprite:setOffset(jellySprite.meta.width / 2, jellySprite.meta.height / 2 + 4)

        table.insert(lineSprites, 1, jellySprite)

        return lineSprites

    else
        return drawableSprite.fromTexture(texture, entity)
    end
end

function respawningBounceJellyfish.rectangle(room, entity)
    local texture = entity.spriteDirectory .. "/idle0"
    local sprite = drawableSprite.fromTexture(texture, entity)

    return sprite:getRectangle()
end

return respawningBounceJellyfish