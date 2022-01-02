local drawableSprite = require("structs.drawable_sprite")

local decal = {}

decal.name = "MaxHelpingHand/HintDecal"

decal.placements = {
    name = "decal",
    data = {
        texture = "1-forsakencity/sign_you_can_go_up",
        scaleX = 1.0,
        scaleY = 1.0,
        foreground = false
    }
}

function decal.depth(room, entity)
    return entity.foreground and -10500 or 9000
end

function decal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("decals/" .. entity.texture, entity)
    sprite:setScale(entity.scaleX, entity.scaleY)
    return sprite
end

return decal