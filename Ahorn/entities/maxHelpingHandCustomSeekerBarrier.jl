module MaxHelpingHandCustomSeekerBarrier

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomSeekerBarrier" CustomSeekerBarrier(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    color::String="FFFFFF", particleColor::String="FFFFFF", transparency::Number=0.15, particleTransparency::Number=0.5, particleDirection::Number=0.0, wavy::Bool=true,
    killSeekers::Bool=true, killJellyfish::Bool=true, disableIfFlag::String="", killHoldableContainerNonSlowFall::Bool=true, killHoldableContainerSlowFall::Bool=true)

const placements = Ahorn.PlacementDict(
    "Seeker Barrier (Custom) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CustomSeekerBarrier,
        "rectangle"
    ),
)

Ahorn.minimumSize(entity::CustomSeekerBarrier) = 8, 8
Ahorn.resizable(entity::CustomSeekerBarrier) = true, true

function Ahorn.selection(entity::CustomSeekerBarrier)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomSeekerBarrier, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.25, 0.25, 0.25, 0.8), (0.0, 0.0, 0.0, 0.0))
end

end
