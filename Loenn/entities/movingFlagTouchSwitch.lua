local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local touchSwitch = {}

touchSwitch.name = "MaxHelpingHand/MovingFlagTouchSwitch"
touchSwitch.depth = 2000
touchSwitch.nodeLimits = {0, -1}
touchSwitch.nodeLineRenderType = "line"
touchSwitch.placements = {
    {
        name = "touch_switch",
        data = {
            flag = "moving_flag_touch_switch",
            icon = "vanilla",
            persistent = false,
            inactiveColor = "5FCDE4",
            movingColor = "FF8080",
            activeColor = "FFFFFF",
            finishColor = "F141DF"
        }
    }
}

touchSwitch.fieldOrder = {"x", "y", "inactiveColor", "movingColor", "activeColor", "finishColor"}

touchSwitch.fieldInformation = {
    inactiveColor = {
        fieldType = "color"
    },
    movingColor = {
        fieldType = "color"
    },
    activeColor = {
        fieldType = "color"
    },
    finishColor = {
        fieldType = "color"
    }
}

local containerTexture = "objects/touchswitch/container"

function touchSwitch.sprite(room, entity)
    local containerSprite = drawableSprite.fromTexture(containerTexture, entity)

    iconResource = "objects/touchswitch/icon00"
    if entity.icon ~= "vanilla" then
        iconResource = "objects/MaxHelpingHand/flagTouchSwitch/" .. entity.icon .."/icon00"
    end

    local iconSprite = drawableSprite.fromTexture(iconResource, entity)

    return {containerSprite, iconSprite}
end

return touchSwitch
