local drawableSprite = require("structs.drawable_sprite")
local drawableFunction = require("structs.drawable_function")
local drawing = require("utils.drawing")
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
        displayOnMap = true
    }
}

function comment.sprite(room, entity)
    if entity.displayOnMap and entity.comment ~= "" then
        -- write the comment on the map
        local font = love.graphics.getFont()
        return drawableFunction.fromFunction(function()
            drawing.callKeepOriginalColor(function()
                love.graphics.setColor({255 / 255, 255 / 255, 255 / 255})
                drawing.printCenteredText(entity.comment, entity.x, entity.y, entity.width, entity.height, font, 1)
            end)
        end)
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
