module MaxHelpingHandRespawningJellyfish

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/RespawningJellyfish" RespawningJellyfish(x::Integer, y::Integer, bubble::Bool=false, tutorial::Bool=false, respawnTime::Number=2.0, spriteDirectory::String="objects/MaxHelpingHand/glider")

const placements = Ahorn.PlacementDict(
    "Respawning Jellyfish (max480's Helping Hand)" => Ahorn.EntityPlacement(
        RespawningJellyfish
    ),
    "Respawning Jellyfish (Floating) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        RespawningJellyfish,
        "point",
        Dict{String, Any}(
            "bubble" => true
        )
    )
)

function Ahorn.selection(entity::RespawningJellyfish)
    x, y = Ahorn.position(entity)
    sprite = get(entity, "spriteDirectory", "objects/MaxHelpingHand/glider") * "/idle0"

    return Ahorn.getSpriteRectangle(sprite, x, y - 4)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RespawningJellyfish, room::Maple.Room)
    sprite = get(entity, "spriteDirectory", "objects/MaxHelpingHand/glider") * "/idle0"
    Ahorn.drawSprite(ctx, sprite, 0, -4)

    if get(entity, "bubble", false)
        curve = Ahorn.SimpleCurve((-7, -1), (7, -1), (0, -6))
        Ahorn.drawSimpleCurve(ctx, curve, (1.0, 1.0, 1.0, 1.0), thickness=1)
    end
end

end