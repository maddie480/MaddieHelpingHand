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
            persistent = false,
            inactiveColor = "5FCDE4",
            activeColor = "FFFFFF",
            finishColor = "F141DF",
            smoke = true,
            inverted = false,
            allowDisable = false,
            hitSound = "event:/game/general/touchswitch_any",
            completeSoundFromSwitch = "event:/game/general/touchswitch_last_cutoff",
            completeSoundFromScene = "event:/game/general/touchswitch_last_oneshot"
        }
    }
}

touchSwitch.fieldInformation = {
    inactiveColor = {
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
