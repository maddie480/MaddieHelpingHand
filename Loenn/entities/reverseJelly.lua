local drawableSprite = require("structs.drawable_sprite")
local drawableLine = require("structs.drawable_line")
local drawing = require("utils.drawing")

local glider = {}

glider.name = "MaxHelpingHand/ReverseJelly"
glider.depth = -5
glider.placements = {
    {
        name = "normal",
        data = {
            spriteDirectory = "MaxHelpingHand/jellies/reversejelly",
            tutorial = false,
            bubble = false,
            glow = false
        }
    },
    {
        name = "floating",
        data = {
            spriteDirectory = "MaxHelpingHand/jellies/reversejelly",
            tutorial = false,
            bubble = true,
            glow = false
        }
    }
}

function glider.sprite(room, entity)
    local bubble = entity.bubble

    if entity.bubble then
        local x, y = entity.x or 0, entity.y or 0
        local points = drawing.getSimpleCurve({x - 11, y - 1}, {x + 11, y - 1}, {x - 0, y - 6})
        local lineSprites = drawableLine.fromPoints(points):getDrawableSprite()
        local jellySprite = drawableSprite.fromTexture(entity.spriteDirectory .. "/idle0", entity)

        table.insert(lineSprites, 1, jellySprite)

        return lineSprites

    else
        return drawableSprite.fromTexture(entity.spriteDirectory .. "/idle0", entity)
    end
end

function glider.rectangle(room, entity)
    local sprite = drawableSprite.fromTexture(entity.spriteDirectory .. "/idle0", entity)

    return sprite:getRectangle()
end

return glider
