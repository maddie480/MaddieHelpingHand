local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local floatingDebris = {}

floatingDebris.name = "MaxHelpingHand/ReskinnableFloatingDebris"
floatingDebris.depth = -5
floatingDebris.placements = {
    name = "floating_debris",
    data = {
        texture = "scenery/debris",
        depth = -5,
        debrisWidth = 8,
        debrisHeight = 8,
        interactWithPlayer = true,
        rotateSpeed = "",
        scrollX = 0,
        scrollY = 0
    }
}

floatingDebris.fieldInformation = {
    depth = {
        fieldType = "integer"
    },
    debrisWidth = {
        fieldType = "integer"
    },
    debrisHeight = {
        fieldType = "integer"
    }
}

function floatingDebris.sprite(room, entity)
    utils.setSimpleCoordinateSeed(entity.x, entity.y)

    local debrisWidth = entity.debrisWidth or 8
    local debrisHeight = entity.debrisHeight or 8

    local sprite = drawableSprite.fromTexture(entity.texture, entity)
    local offsetX = math.random(0, sprite.meta.width / debrisWidth - 1) * debrisWidth

    -- Manually offset the sprite, otherwise it will justify with the original image size
    sprite:useRelativeQuad(offsetX, 0, debrisWidth, debrisHeight)
    sprite:setJustification(0.0, 0.0)
    sprite:addPosition(-debrisWidth / 2, -debrisHeight / 2)

    return sprite
end

function floatingDebris.selection(room, entity)
    local debrisWidth = entity.debrisWidth or 8
    local debrisHeight = entity.debrisHeight or 8
    return utils.rectangle(entity.x - (debrisWidth / 2), entity.y - (debrisHeight / 2), debrisWidth, debrisHeight)
end

return floatingDebris
