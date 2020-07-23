module MaxHelpingHandKevinBarrier

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/KevinBarrier" KevinBarrier(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, color::String="62222b")

const placements = Ahorn.PlacementDict(
    "Kevin Barrier (max480's Helping Hand)" => Ahorn.EntityPlacement(
        KevinBarrier,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::KevinBarrier) = 8, 8
Ahorn.resizable(entity::KevinBarrier) = true, true

function Ahorn.selection(entity::KevinBarrier)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KevinBarrier, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    color = Ahorn.argb32ToRGBATuple(parse(Int, get(entity.data, "color", "62222b"), base=16))[1:3] ./ 255
    Ahorn.drawRectangle(ctx, 0, 0, width, height, color, (0.0, 0.0, 0.0, 0.0))
end

end
