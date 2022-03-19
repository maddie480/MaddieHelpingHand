local strawberrySeed = {}

strawberrySeed.name = "MaxHelpingHand/MultiRoomStrawberrySeed"
strawberrySeed.depth = -100

strawberrySeed.placements = {
    name = "seed",
    data = {
        strawberryName = "multi_room_strawberry",
        sprite = "MaxHelpingHand/miniberry/miniberry",
        ghostSprite = "MaxHelpingHand/miniberry/ghostminiberry",
        index = -1,
        displaySeedCount = false
    }
}

strawberrySeed.fieldInformation = {
    sprite = {
        options = { "strawberry/seed", "MaxHelpingHand/miniberry/miniberry" }
    },
    ghostSprite = {
        options = { "ghostberry/seed", "MaxHelpingHand/miniberry/ghostminiberry" }
    }
}

function strawberrySeed.texture(room, entity)
    return "collectables/" .. entity.sprite .. "00"
end

return strawberrySeed
