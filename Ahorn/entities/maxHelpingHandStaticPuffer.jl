module MaxHelpingHandStaticPuffer

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/StaticPuffer" StaticPuffer(x::Integer, y::Integer, right::Bool=false, downboostTolerance::Int=-1)

const placements = Ahorn.PlacementDict(
    "Static Puffer (Right) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        StaticPuffer,
        "point",
        Dict{String, Any}(
            "right" => true
        )
    ),
    "Static Puffer (Left) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        StaticPuffer,
        "point",
        Dict{String, Any}(
            "right" => false
        )
    )
)

sprite = "objects/puffer/idle00"

function Ahorn.selection(entity::StaticPuffer)
    x, y = Ahorn.position(entity)
    scaleX = get(entity, "right", false) ? 1 : -1

    return Ahorn.getSpriteRectangle(sprite, x, y, sx=scaleX)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StaticPuffer, room::Maple.Room)
    scaleX = get(entity, "right", false) ? 1 : -1

    Ahorn.drawSprite(ctx, sprite, 0, 0, sx=scaleX)
end

end
