local drawableSpriteStruct = require("structs.drawable_sprite")

local lava = {}

lava.name = "MaxHelpingHand/SidewaysLava"
lava.depth = 0
lava.placements = {
    name = "lava",
    data = {
        intro = false,
        lavaMode = "LeftToRight",
        speedMultiplier = 1.0
    }
}

lava.fieldInformation = {
    lavaMode = {
        options = { "LeftToRight", "RightToLeft", "Sandwich" },
        editable = false
    }
}

function lava.rotation(room, entity)
    if entity.lavaMode == "RightToLeft" then
        return - math.pi / 2
    else
        return math.pi / 2
    end
end

function lava.texture(room, entity)
    if entity.lavaMode == "Sandwich" then
        return "@Internal@/lava_sandwich"
    else
        return "@Internal@/rising_lava"
    end
end

return lava
