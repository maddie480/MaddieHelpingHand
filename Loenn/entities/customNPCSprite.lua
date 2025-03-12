local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local everestNpc = {}

everestNpc.name = "MaxHelpingHand/CustomNPCSprite"
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
        dialogId = "",
        onlyOnce = true,
        endLevel = false,
        flipX = false,
        flipY = false,
        approachWhenTalking = false,
        approachDistance = 16,
        indicatorOffsetX = 0,
        indicatorOffsetY = 0,
        spriteName = "bird",
        onlyIfFlag = "",
        setFlag = "",
        onlyIfFlagInverted = false,
        setFlagInverted = false,
        autoSkipEnabled = false,
        customFont = ""
    }
}

local borderColor = {1.0, 1.0, 1.0, 1.0}
local fillColor = {1.0, 1.0, 1.0, 0.8}

function everestNpc.nodeSprite(room, entity, node, index)
    local rectangle = utils.rectangle(node.x, node.y, 8, 8)
    return drawableRectangle.fromRectangle("bordered", rectangle, fillColor, borderColor):getDrawableSprite()
end

everestNpc.texture = "ahorn/MaxHelpingHand/custom_npc_xml"
everestNpc.nodeTexture = "ahorn/MaxHelpingHand/orange_rectangle"

return everestNpc
