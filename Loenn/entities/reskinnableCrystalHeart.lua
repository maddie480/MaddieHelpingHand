local heart = {}

heart.name = "MaxHelpingHand/ReskinnableCrystalHeart"
heart.depth = -2000000
heart.placements = {
    name = "crystal_heart",
    data = {
        fake = false,
        removeCameraTriggers = false,
        fakeHeartDialog = "CH9_FAKE_HEART",
        keepGoingDialog = "CH9_KEEP_GOING",
        sprite = "",
        ghostSprite = "",
        particleColor = "",
        flagOnCollect = "",
        rotation = 0,
        flagInverted = false,
        disableGhostSprite = false
    }
}

heart.fieldOrder = {
    "x", "y",
    "fakeHeartDialog", "keepGoingDialog",
    "sprite", "ghostSprite",
    "particleColor", "flagOnCollect",
    "rotation", "_spacer",
    "disableGhostSprite", "fake", "flagInverted", "removeCameraTriggers"
}

heart.fieldInformation = {
    sprite = {
        options = { "", "heartgem0", "heartgem1", "heartgem2", "heartgem3" }
    },
    ghostSprite = {
        options = { "", "heartgem0", "heartgem1", "heartgem2", "heartgem3" }
    },
    particleColor = {
        fieldType = "color",
        allowEmpty = true
    },
    rotation = {
        default = 0
    },

    -- a hack borrowed from lönn's setting window to let us insert a spacer,
    -- this way the menu is ordered neatly
    _spacer = {
        fieldType = "spacer"
    }
}


function heart.texture(room, entity)
    if entity.sprite == "heartgem1" then
        return "collectables/heartGem/1/00"
    elseif entity.sprite == "heartgem2" then
        return "collectables/heartGem/2/00"
    elseif entity.sprite == "heartgem3" then
        return "collectables/heartGem/3/00"
    else
        return "collectables/heartGem/0/00"
    end
end
function heart.rotation(room, entity)
    return entity.rotation and (entity.rotation / 180 * math.pi) or 0
end

return heart
