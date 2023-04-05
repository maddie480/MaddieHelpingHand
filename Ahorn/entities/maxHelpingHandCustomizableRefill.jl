module MaxHelpingHandCustomizableRefill

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomizableRefill" CustomizableRefill(x::Integer, y::Integer, twoDash::Bool=false, oneUse::Bool=false, respawnTime::Number=2.5, sprite::String="",
    shatterParticleColor1::String="", shatterParticleColor2::String="", glowParticleColor1::String="", glowParticleColor2::String="", wave::Bool=true)

const placements = Ahorn.PlacementDict(
    "Refill (Customizable) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CustomizableRefill
    ),

    "Refill (Two Dashes, Customizable) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CustomizableRefill,
        "point",
        Dict{String, Any}(
            "twoDash" => true
        )
    )
)

spriteOneDash = "objects/refill/idle00"
spriteTwoDash = "objects/refillTwo/idle00"

function getSprite(entity::CustomizableRefill)
    twoDash = get(entity.data, "twoDash", false)
    sprite = get(entity.data, "sprite", "")

    return sprite != "" ? "objects/MaxHelpingHand/refill/" * sprite * "/idle00" : (twoDash ? spriteTwoDash : spriteOneDash)
end

function Ahorn.selection(entity::CustomizableRefill)
    x, y = Ahorn.position(entity)
    sprite = getSprite(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomizableRefill, room::Maple.Room)
    sprite = getSprite(entity)
    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
