module MaxHelpingHandExpandTriggerController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/ExpandTriggerController" ExpandTriggerController(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Expand Trigger Controller (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ExpandTriggerController
    )
)

function Ahorn.selection(entity::ExpandTriggerController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ExpandTriggerController, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/expand_trigger_controller", 0, 0)

end
