local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local touchSwitch = {}

touchSwitch.name = "MaxHelpingHand/ReskinnableTouchSwitch"
touchSwitch.depth = 2000
touchSwitch.placements = {
    {
        name = "touch_switch",
        data = {
            icon = "objects/touchswitch/icon",
            borderTexture = "objects/touchswitch/container",
            inactiveColor = "5fcde4",
            activeColor = "ffffff",
            finishColor = "f141df"
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
    },
    icon = {
        options = {
            "objects/touchswitch/icon",
            "objects/MaxHelpingHand/flagTouchSwitch/tall/icon",
            "objects/MaxHelpingHand/flagTouchSwitch/triangle/icon",
            "objects/MaxHelpingHand/flagTouchSwitch/circle/icon",
            "objects/MaxHelpingHand/flagTouchSwitch/diamond/icon",
            "objects/MaxHelpingHand/flagTouchSwitch/double/icon",
            "objects/MaxHelpingHand/flagTouchSwitch/heart/icon",
            "objects/MaxHelpingHand/flagTouchSwitch/square/icon",
            "objects/MaxHelpingHand/flagTouchSwitch/wide/icon",
            "objects/MaxHelpingHand/flagTouchSwitch/winged/icon"
        }
    }
}

function touchSwitch.sprite(room, entity)
    local containerSprite = drawableSprite.fromTexture(entity.borderTexture, entity)
    local iconSprite = drawableSprite.fromTexture(entity.icon .. "00", entity)

    return {containerSprite, iconSprite}
end

return touchSwitch
