module MaxHelpingHandSpikeRefillController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/SpikeRefillController" SpikeRefillController(x::Integer, y::Integer, flag::String="", flagInverted::Bool=false)

const placements = Ahorn.PlacementDict(
    "Spike Refill Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        SpikeRefillController
    )
)

function Ahorn.selection(entity::SpikeRefillController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpikeRefillController, room::Maple.Room) = Ahorn.drawImage(ctx, "objects/MaxHelpingHand/spikeRefillController/controller", -12, -12)

end
