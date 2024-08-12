module MaxHelpingHandMiniHeartDoorUnfixController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/MiniHeartDoorUnfixController" MiniHeartDoorUnfixController(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Mini Heart Door Unfix Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        MiniHeartDoorUnfixController
    )
)

function Ahorn.selection(entity::MiniHeartDoorUnfixController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MiniHeartDoorUnfixController, room::Maple.Room) = Ahorn.drawImage(ctx, "ahorn/MaxHelpingHand/miniheartdoorunfix", -12, -12)

end