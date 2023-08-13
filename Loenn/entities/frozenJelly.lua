local drawableSprite = require("structs.drawable_sprite")

local glider = {}

glider.name = "MaxHelpingHand/FrozenJelly"
glider.depth = -5
glider.placements = {
    name = "normal",
    data = {
        spriteDirectory = "MaxHelpingHand/jellies/frozenjelly",
        tutorial = false,
        glow = false
    }
}

function glider.sprite(room, entity)
    return drawableSprite.fromTexture(entity.spriteDirectory .. "/idle0", entity)
end

function glider.rectangle(room, entity)
    local sprite = drawableSprite.fromTexture(entity.spriteDirectory .. "/idle0", entity)

    return sprite:getRectangle()
end

return glider
