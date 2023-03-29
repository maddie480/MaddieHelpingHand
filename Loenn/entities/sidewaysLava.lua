local drawableSpriteStruct = require("structs.drawable_sprite")

local lava = {}

lava.name = "MaxHelpingHand/SidewaysLava"
lava.depth = 0
lava.placements = {
    name = "lava",
    data = {
        intro = false,
        lavaMode = "LeftToRight",
        speedMultiplier = 1.0,
        hotSurfaceColor = "ff8933",
        hotEdgeColor = "f25e29",
        hotCenterColor = "d01c01",
        coldSurfaceColor = "33ffe7",
        coldEdgeColor = "4ca2eb",
        coldCenterColor = "0151d0",
        sound = "event:/game/09_core/rising_threat",
        forceCoreMode = "None"
    }
}

lava.fieldInformation = {
    lavaMode = {
        options = { "LeftToRight", "RightToLeft", "Sandwich" },
        editable = false
    },
    hotSurfaceColor = {
        fieldType = "color"
    },
    hotEdgeColor = {
        fieldType = "color"
    },
    hotCenterColor = {
        fieldType = "color"
    },
    coldSurfaceColor = {
        fieldType = "color"
    },
    coldEdgeColor = {
        fieldType = "color"
    },
    coldCenterColor = {
        fieldType = "color"
    },
    forceCoreMode = {
        options = { "None", "Cold", "Hot" },
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
