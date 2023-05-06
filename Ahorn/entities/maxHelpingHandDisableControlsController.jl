module MaxHelpingHandDisableControlsController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/DisableControlsController" DisableControlsController(x::Integer, y::Integer,
    up::Bool=false, down::Bool=false, left::Bool=false, right::Bool=false, jump::Bool=false, grab::Bool=false, dash::Bool=false, onlyIfFlag::String="")

const placements = Ahorn.PlacementDict(
    "Disable Controls Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        DisableControlsController
    )
)

function Ahorn.selection(entity::DisableControlsController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DisableControlsController, room::Maple.Room) = Ahorn.drawImage(ctx, "ahorn/MaxHelpingHand/disable_controls", -12, -12)

end
