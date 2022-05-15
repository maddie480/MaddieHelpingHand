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
        flagInverted = false,
        disableGhostSprite = false
    }
}

heart.fieldInformation = {
    sprite = {
        options = { "", "heartgem0", "heartgem1", "heartgem2", "heartgem3" }
    },
    ghostSprite = {
        options = { "", "heartgem0", "heartgem1", "heartgem2", "heartgem3" }
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

return heart
