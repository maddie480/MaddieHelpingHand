local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local touchSwitch = {}

touchSwitch.name = "MaxHelpingHand/FlagTouchSwitch"
touchSwitch.depth = 2000
touchSwitch.placements = {
    {
        name = "touch_switch",
        data = {
            flag = "flag_touch_switch",
            icon = "vanilla",
            animationLength = 6,
            borderTexture = "",
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
            hideIfFlag = ""
        }
    }
}

function touchSwitch.fieldOrder(entity)
    local fieldOrder = {"x", "y", "inactiveColor", "activeColor", "finishColor", "hitSound", "completeSoundFromSwitch", "completeSoundFromScene"}

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

local containerTexture = "objects/touchswitch/container"

function touchSwitch.sprite(room, entity)
    local borderTexture = entity.borderTexture ~= "" and entity.borderTexture or containerTexture
    local containerSprite = drawableSprite.fromTexture(borderTexture, entity)

    local iconResource = "objects/touchswitch/icon00"
    if entity.icon ~= "vanilla" then
        iconResource = "objects/MaxHelpingHand/flagTouchSwitch/" .. entity.icon .."/icon00"
    end

    local iconSprite = drawableSprite.fromTexture(iconResource, entity)

    return {containerSprite, iconSprite}
end

return touchSwitch
