module MaxHelpingHandRespawningBounceJellyfish

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/RespawningBounceJellyfish" RespawningBounceJellyfish(x::Integer, y::Integer, platform::Bool=true, soulBound::Bool=true, baseDashCount::Integer=1,
    respawnTime::Number=2.0, spriteDirectory::String="objects/MaxHelpingHand/glider")

const placements = Ahorn.PlacementDict(
    "Respawning Bounce Jellyfish (Bounce Helper + Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        RespawningBounceJellyfish
    )
)

function Ahorn.selection(entity::RespawningBounceJellyfish)
    x, y = Ahorn.position(entity)
    sprite = get(entity, "spriteDirectory", "objects/MaxHelpingHand/glider") * "/idle0"

    return Ahorn.getSpriteRectangle(sprite, x, y - 4)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RespawningBounceJellyfish, room::Maple.Room)
    sprite = get(entity, "spriteDirectory", "objects/MaxHelpingHand/glider") * "/idle0"
    Ahorn.drawSprite(ctx, sprite, 0, -4)

    if get(entity, "platform", false)
        curve = Ahorn.SimpleCurve((-7, -1), (7, -1), (0, -6))
        Ahorn.drawSimpleCurve(ctx, curve, (1.0, 1.0, 1.0, 1.0), thickness=1)
    end
end


end