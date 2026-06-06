local refill = {}

refill.name = "MaxHelpingHand/CustomizableRefill"
refill.depth = -100
refill.placements = {
    {
        name = "one_dash",
        data = {
            oneUse = false,
            twoDash = false,
            respawnTime = 2.5,
            sprite = "",
            shatterParticleColor1 = "",
            shatterParticleColor2 = "",
            glowParticleColor1 = "",
            glowParticleColor2 = "",
            rotation = 0,
            wave = true
        }
    },
    {
        name = "two_dashes",
        data = {
            oneUse = false,
            twoDash = true,
            respawnTime = 2.5,
            sprite = "",
            shatterParticleColor1 = "",
            shatterParticleColor2 = "",
            glowParticleColor1 = "",
            glowParticleColor2 = "",
            rotation = 0,
            wave = true
        }
    }
}

refill.fieldOrder = {
    "x", "y",
    "sprite", "respawnTime",
    "shatterParticleColor1", "shatterParticleColor2",
    "glowParticleColor1", "glowParticleColor2",
    "rotation", "_spacer",
    "oneUse", "twoDash",
    "wave"
}

refill.fieldInformation = {
    shatterParticleColor1 = {
        fieldType = "color",
        allowEmpty = true
    },
    shatterParticleColor2 = {
        fieldType = "color",
        allowEmpty = true
    },
    glowParticleColor1 = {
        fieldType = "color",
        allowEmpty = true
    },
    glowParticleColor2 = {
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

function refill.texture(room, entity)
    return entity.sprite ~= "" and "objects/MaxHelpingHand/refill/" .. entity.sprite .. "/idle00" or
        (entity.twoDash and "objects/refillTwo/idle00" or "objects/refill/idle00")
end

function refill.rotation(room, entity)
    return entity.rotation and (entity.rotation / 180 * math.pi) or 0
end

return refill
