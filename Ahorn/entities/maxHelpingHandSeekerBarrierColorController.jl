module MaxHelpingHandSeekerBarrierColorController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/SeekerBarrierColorController" SeekerBarrierColorController(x::Integer, y::Integer,
    color::String="FFFFFF", particleColor::String="FFFFFF", transparency::Number=0.15, particleTransparency::Number=0.5, persistent::Bool=false, particleDirection::Number=0.0, depth::String="")

const placements = Ahorn.PlacementDict(
    "Seeker Barrier Color Controller (max480's Helping Hand)" => Ahorn.EntityPlacement(
        SeekerBarrierColorController
    )
)

Ahorn.editingOrder(entity::SeekerBarrierColorController) = String["x", "y", "color", "transparency", "particleColor", "particleTransparency"]

function Ahorn.selection(entity::SeekerBarrierColorController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SeekerBarrierColorController, room::Maple.Room) = Ahorn.drawImage(ctx, Ahorn.Assets.northernLights, -12, -12)

end
