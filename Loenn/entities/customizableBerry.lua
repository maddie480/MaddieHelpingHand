local strawberry = {}

strawberry.name = "MaxHelpingHand/CustomizableBerry"
strawberry.depth = -100

strawberry.texture = "collectables/strawberry/normal00"
function strawberry.rotation(room, entity)
    return entity.rotation and (entity.rotation / 180 * math.pi) or 0
end

strawberry.placements = {
    name = "berry",
    data = {
        checkpointID = -1,
        order = -1,
        strawberrySprite = "strawberry",
        ghostberrySprite = "ghostberry",
        strawberryPulseSound = "event:/game/general/strawberry_pulse",
        strawberryBlueTouchSound = "event:/game/general/strawberry_blue_touch",
        strawberryTouchSound = "event:/game/general/strawberry_touch",
        strawberryGetSound = "event:/game/general/strawberry_get",
        pulseEnabled = true,
        spotlightEnabled = true,
        countTowardsTotal = true,
        moonBerrySound = false,
        particleColor1 = "FF8563",
        particleColor2 = "FFF4A8",
        ghostParticleColor1 = "6385FF",
        ghostParticleColor2 = "72F0FF",
        rotation = 0,
        visibleIfFlag = ""
    }
}

strawberry.fieldOrder = {
    "x", "y",
    "strawberrySprite", "ghostberrySprite",
    "strawberryPulseSound", "strawberryTouchSound",
    "strawberryBlueTouchSound", "strawberryGetSound",
    "particleColor1", "particleColor2",
    "ghostParticleColor1", "ghostParticleColor2",
    "checkpointID", "order",
    "rotation", "visibleIfFlag",
    "moonBerrySound", "pulseEnabled", "spotlightEnabled"
}

strawberry.ignoredFields = {"_id", "_name", "countTowardsTotal"}

strawberry.fieldInformation = {
    particleColor1 = {
        fieldType = "color"
    },
    particleColor2 = {
        fieldType = "color"
    },
    ghostParticleColor1 = {
        fieldType = "color"
    },
    ghostParticleColor2 = {
        fieldType = "color"
    },
    order = {
        fieldType = "integer",
    },
    checkpointID = {
        fieldType = "integer"
    },
    rotation = {
        default = 0
    }
}

return strawberry
