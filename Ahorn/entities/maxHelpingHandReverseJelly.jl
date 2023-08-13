module MaxHelpingHandReverseJelly

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/ReverseJelly" ReverseJelly(x::Integer, y::Integer, bubble::Bool=false,
    tutorial::Bool=false, spriteDirectory::String="MaxHelpingHand/jellies/reversejelly", glow::Bool=false)

const placements = Ahorn.PlacementDict(
    "Reverse Jelly (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        ReverseJelly
    ),
    "Reverse Jelly (Floating) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        ReverseJelly,
        "point",
        Dict{String, Any}(
            "bubble" => true
        )
    )
)

function Ahorn.selection(entity::ReverseJelly)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(entity.spriteDirectory * "/idle0", x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReverseJelly, room::Maple.Room)
    Ahorn.drawSprite(ctx, entity.spriteDirectory * "/idle0", 0, 0)

    if get(entity, "bubble", false)
        curve = Ahorn.SimpleCurve((-7, -1), (7, -1), (0, -6))
        Ahorn.drawSimpleCurve(ctx, curve, (1.0, 1.0, 1.0, 1.0), thickness=1)
    end
end

end
