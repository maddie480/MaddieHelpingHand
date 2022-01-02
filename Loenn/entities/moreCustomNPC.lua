local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local everestNpc = {}

everestNpc.name = "MaxHelpingHand/MoreCustomNPC"
everestNpc.depth = 100
everestNpc.justification = {0.5, 1.0}
everestNpc.nodeLimits = {0, 2}
everestNpc.nodeLineRenderType = "line"
everestNpc.fieldInformation = {
    spriteRate = {
        fieldType = "integer",
    },
    approachDistance = {
        fieldType = "integer",
    },
    indicatorOffsetX = {
        fieldType = "integer",
    },
    indicatorOffsetY = {
        fieldType = "integer",
    }
}

everestNpc.placements = {
    name = "npc",
    data = {
        sprite = "player/idle",
        spriteRate = 1,
        dialogId = "",
        onlyOnce = true,
        endLevel = false,
        flipX = false,
        flipY = false,
        approachWhenTalking = false,
        approachDistance = 16,
        indicatorOffsetX = 0,
        indicatorOffsetY = 0,
        frames = "",
        onlyIfFlag = "",
        setFlag = ""
    }
}

local borderColor = {1.0, 1.0, 1.0, 1.0}
local fillColor = {1.0, 1.0, 1.0, 0.8}

function everestNpc.nodeSprite(room, entity, node, index)
    local rectangle = utils.rectangle(node.x, node.y, 8, 8)
    return drawableRectangle.fromRectangle("bordered", rectangle, fillColor, borderColor):getDrawableSprite()
end

function everestNpc.scale(room, entity)
    local scaleX = entity.flipX and -1 or 1
    local scaleY = entity.flipY and -1 or 1

    return scaleX, scaleY
end

function everestNpc.texture(room, entity)
    local texture = string.format("characters/%s00", entity.sprite or "")

    return texture
end

return everestNpc
