module MaxHelpingHandOneWayInvisibleBarrierHorizontal

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/OneWayInvisibleBarrierHorizontal" OneWayInvisibleBarrierHorizontal(x::Integer, y::Integer, height::Integer=Maple.defaultBlockHeight, 
    left::Bool=true)


const placements = Ahorn.PlacementDict(
    "One-Way Invisible Barrier (Left, max480's Helping Hand)" => Ahorn.EntityPlacement(
        OneWayInvisibleBarrierHorizontal,
        "rectangle",
        Dict{String, Any}(
            "left" => true
        )
    ),
    "One-Way Invisible Barrier (Right, max480's Helping Hand)" => Ahorn.EntityPlacement(
        OneWayInvisibleBarrierHorizontal,
        "rectangle",
        Dict{String, Any}(
            "left" => false
        )
    )
)

Ahorn.minimumSize(entity::OneWayInvisibleBarrierHorizontal) = 8, 8
Ahorn.resizable(entity::OneWayInvisibleBarrierHorizontal) = false, true

Ahorn.selection(entity::OneWayInvisibleBarrierHorizontal) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::OneWayInvisibleBarrierHorizontal, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.4, 0.4, 0.4, 0.8), (0.0, 0.0, 0.0, 0.0))
end

end
