module MaxHelpingHandCustomizableGlassFallingBlock

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomizableGlassFallingBlock" CustomizableGlassFallingBlock(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    behindFgTiles::Bool=false, climbFall::Bool=true)

const placements = Ahorn.PlacementDict(
    "Customizable Glass Falling Block (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CustomizableGlassFallingBlock,
        "rectangle"
    )
)

Ahorn.minimumSize(entity::CustomizableGlassFallingBlock) = 8, 8
Ahorn.resizable(entity::CustomizableGlassFallingBlock) = true, true

Ahorn.selection(entity::CustomizableGlassFallingBlock) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomizableGlassFallingBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (1.0, 1.0, 1.0, 0.5), (1.0, 1.0, 1.0, 0.5))
end

end
