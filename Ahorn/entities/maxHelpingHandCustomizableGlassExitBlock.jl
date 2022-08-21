module MaxHelpingHandCustomizableGlassExitBlock

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomizableGlassExitBlock" CustomizableGlassExitBlock(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    behindFgTiles::Bool=false, playerMustEnterFirst::Bool=false)

const placements = Ahorn.PlacementDict(
    "Customizable Glass Exit Block (max480's Helping Hand)" => Ahorn.EntityPlacement(
        CustomizableGlassExitBlock,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::CustomizableGlassExitBlock) = 8, 8
Ahorn.resizable(entity::CustomizableGlassExitBlock) = true, true

Ahorn.selection(entity::CustomizableGlassExitBlock) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomizableGlassExitBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (1.0, 1.0, 1.0, 0.5), (1.0, 1.0, 1.0, 0.5))
end

end
