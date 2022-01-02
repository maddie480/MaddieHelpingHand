local drawableRectangle = require("structs.drawable_rectangle")

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
        setFlag = ""
    }
}

everestNpc.texture = "ahorn/MaxHelpingHand/custom_npc_xml"
everestNpc.nodeTexture = "ahorn/MaxHelpingHand/orange_rectangle"

return everestNpc
