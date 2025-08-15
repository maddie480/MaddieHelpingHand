local drawableSprite = require("structs.drawable_sprite")
local drawableRectangle = require("structs.drawable_rectangle")

local touchSwitch = {}

touchSwitch.name = "MaxHelpingHand/FlagTouchSwitchWall"
touchSwitch.depth = 2000
touchSwitch.minimumSize = {8, 8}
touchSwitch.placements = {
    {
        name = "touch_switch",
        data = {
            width = 16,
            height = 16,
            flag = "flag_touch_switch",
            icon = "vanilla",
            animationLength = 6,
            persistent = false,
            inactiveColor = "5FCDE4",
            activeColor = "FFFFFF",
            finishColor = "F141DF",
            smoke = true,
            inverted = false,
            allowDisable = false,
            playerCanActivate = true,
            hitSound = "event:/game/general/touchswitch_any",
            completeSoundFromSwitch = "event:/game/general/touchswitch_last_cutoff",
            completeSoundFromScene = "event:/game/general/touchswitch_last_oneshot",
            hideIfFlag = "",
            legacyFlagMode = false
        }
    }
}

function touchSwitch.fieldOrder(entity)
    local fieldOrder = {"x", "y", "width", "height", "inactiveColor", "activeColor", "finishColor", "hitSound", "completeSoundFromSwitch", "completeSoundFromScene"}

    -- only include animationLength to fieldOrder if the field exists, otherwise it will appear as nil in the entity properties window
    if entity.animationLength ~= nil then
        table.insert(fieldOrder, "animationLength")
    end

    return fieldOrder
end

touchSwitch.fieldInformation = {
    inactiveColor = {
        fieldType = "color"
    },
    activeColor = {
        fieldType = "color"
    },
    finishColor = {
        fieldType = "color"
    },
    icon = {
        options = { "vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged", "cross", "drop", "hourglass", "split", "star", "triple" }
    },
    animationLength = {
        fieldType = "integer"
    }
}

function touchSwitch.sprite(room, entity)
    local containerSprite = drawableRectangle.fromRectangle('bordered', entity.x, entity.y, entity.width, entity.height, {0.0, 0.0, 0.0, 0.3}, {1.0, 1.0, 1.0, 0.5})

    local iconResource = "objects/touchswitch/icon00"
    if entity.icon ~= "vanilla" then
        iconResource = "objects/MaxHelpingHand/flagTouchSwitch/" .. entity.icon .."/icon00"
    end

    local iconSprite = drawableSprite.fromTexture(iconResource, entity)
    iconSprite:setPosition(entity.x + entity.width / 2, entity.y + entity.height / 2)

    return {containerSprite, iconSprite}
end

return touchSwitch
