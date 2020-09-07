module MaxHelpingHandCustomizableGlassBlock

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomizableGlassBlock" CustomizableGlassBlock(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, behindFgTiles::Bool=false)

const placements = Ahorn.PlacementDict(
    "Customizable Glass Block (max480's Helping Hand)" => Ahorn.EntityPlacement(
        CustomizableGlassBlock,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::CustomizableGlassBlock) = 8, 8
Ahorn.resizable(entity::CustomizableGlassBlock) = true, true

Ahorn.selection(entity::CustomizableGlassBlock) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomizableGlassBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (1.0, 1.0, 1.0, 0.5), (1.0, 1.0, 1.0, 0.5))
end

end
