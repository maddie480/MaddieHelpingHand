local strawberry = {}

strawberry.name = "MaxHelpingHand/SecretBerry"
strawberry.depth = -100

strawberry.texture = "collectables/moonBerry/normal00"

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
        countTowardsTotal = false,
        particleColor1 = "FF8563",
        particleColor2 = "FFF4A8",
        ghostParticleColor1 = "6385FF",
        ghostParticleColor2 = "72F0FF"
    }
}

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
    }
}

return strawberry
