local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")
local entities = require("entities")

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
    {
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
            setFlag = "",
            autoSkipEnabled = false
        }
    },
    {
        name = "theo",
        data = {
            sprite = "theo/theo",
            spriteRate = 10,
            dialogId = "",
            onlyOnce = true,
            endLevel = false,
            flipX = false,
            flipY = false,
            approachWhenTalking = false,
            approachDistance = 16,
            indicatorOffsetX = 0,
            indicatorOffsetY = -5,
            frames = "0-9",
            onlyIfFlag = "",
            setFlag = "",
            autoSkipEnabled = false
        }
    },
    {
        name = "oshiro",
        data = {
            sprite = "oshiro/oshiro",
            spriteRate = 12,
            dialogId = "",
            onlyOnce = true,
            endLevel = false,
            flipX = false,
            flipY = false,
            approachWhenTalking = false,
            approachDistance = 16,
            indicatorOffsetX = 0,
            indicatorOffsetY = -10,
            frames = "31-41",
            onlyIfFlag = "",
            setFlag = "",
            autoSkipEnabled = false
        }
    },
    {
        name = "badeline_boss",
        data = {
            sprite = "badelineBoss/boss",
            spriteRate = 17,
            dialogId = "",
            onlyOnce = true,
            endLevel = false,
            flipX = false,
            flipY = false,
            approachWhenTalking = false,
            approachDistance = 16,
            indicatorOffsetX = 0,
            indicatorOffsetY = -5,
            frames = "0-23",
            onlyIfFlag = "",
            setFlag = "",
            autoSkipEnabled = false
        }
    },
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
    local spriteName = entity.sprite or ""

    if spriteName == "oshiro/oshiro" then
        return "characters/oshiro/oshiro31"
    end

    return string.format("characters/%s00", entity.sprite or "")
end

-- Add presets for frequently used Everest NPCs
local customNPC = entities.registeredEntities["everest/npc"]

if customNPC.placements.name then
    -- turn "placements" into a table to be able to add another
    customNPC.placements = { customNPC.placements }
end

table.insert(customNPC.placements, {
    name = "maxhelpinghand_granny",
    data = {
        sprite = "oldlady/idle",
        spriteRate = 7,
        dialogId = "",
        onlyOnce = true,
        endLevel = false,
        flipX = false,
        flipY = false,
        approachWhenTalking = false,
        approachDistance = 16,
        indicatorOffsetX = 0,
        indicatorOffsetY = -2
    }
})

table.insert(customNPC.placements, {
    name = "maxhelpinghand_invisible",
    data = {
        sprite = "",
        spriteRate = 1,
        dialogId = "",
        onlyOnce = true,
        endLevel = false,
        flipX = false,
        flipY = false,
        approachWhenTalking = false,
        approachDistance = 16,
        indicatorOffsetX = 0,
        indicatorOffsetY = -16
    }
})

return everestNpc
