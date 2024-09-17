local drawableSprite = require("structs.drawable_sprite")
local drawableText = require("structs.drawable_text")
local utils = require("utils")

local comment = {}

comment.name = "MaxHelpingHand/Comment"
comment.depth = -100000

comment.placements = {
    name = "comment",
    data = {
        width = 16,
        height = 16,
        comment = "",
        textColor = "FFFFFF",
        displayOnMap = true
    }
}

comment.fieldInformation = {
    textColor = {
        fieldType = "color"
    }
}

local function getTextColor(entity)
    local success, r, g, b = utils.parseHexColor(entity.textColor or "FFFFFF")
    return success and {r, g, b} or {1, 1, 1}
end

function comment.sprite(room, entity)
    if entity.displayOnMap and entity.comment ~= "" then
        -- write the comment on the map
        return drawableText.fromText(entity.comment, entity.x, entity.y, entity.width, entity.height, nil, 1, getTextColor(entity))
    else
        -- render a comment bubble sprite instead
        return drawableSprite.fromTexture("ahorn/MaxHelpingHand/comment", entity)
    end
end

function comment.canResize(room, entity)
    if entity.displayOnMap and entity.comment ~= "" then
        -- text zone is resizable
        return true, true
    else
        -- size is fixed at 24x24 (the size of the bubble sprite)
        return false, false
    end
end

function comment.selection(room, entity)
    if entity.displayOnMap and entity.comment ~= "" then
        -- text zone is resizable
        return utils.rectangle(entity.x, entity.y, entity.width, entity.height)
    else
        -- size is fixed at 24x24 (the size of the bubble sprite)
        return utils.rectangle(entity.x - 12, entity.y - 12, 24, 24)
    end
end

return comment
