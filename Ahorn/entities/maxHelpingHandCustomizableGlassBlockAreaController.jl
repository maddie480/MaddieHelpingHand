module MaxHelpingHandCustomizableGlassBlockAreaController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomizableGlassBlockAreaController" CustomizableGlassBlockAreaController(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    starColors::String="ff7777,77ff77,7777ff,ff77ff,77ffff,ffff77", bgColor::String="302040", wavy::Bool=false)

const placements = Ahorn.PlacementDict(
    "Customizable Glass Block Area Controller (max480's Helping Hand)" => Ahorn.EntityPlacement(
        CustomizableGlassBlockAreaController
    )
)

Ahorn.minimumSize(entity::CustomizableGlassBlockAreaController) = 8, 8
Ahorn.resizable(entity::CustomizableGlassBlockAreaController) = true, true

Ahorn.selection(entity::CustomizableGlassBlockAreaController) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomizableGlassBlockAreaController, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.4, 0.4, 1.0, 0.4), (0.4, 0.4, 1.0, 1.0))
end

end
