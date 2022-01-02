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
            wave = true
        }
    }
}

function refill.texture(room, entity)
    return entity.sprite ~= "" and "objects/MaxHelpingHand/refill/" .. entity.sprite .. "/idle00" or
        (entity.twoDash and "objects/refillTwo/idle00" or "objects/refill/idle00")
end

return refill
