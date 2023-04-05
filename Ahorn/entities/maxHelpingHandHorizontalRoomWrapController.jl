module MaxHelpingHandHorizontalRoomWrapController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/HorizontalRoomWrapController" HorizontalRoomWrapController(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Horizontal Room Wrap Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        HorizontalRoomWrapController
    )
)

function Ahorn.selection(entity::HorizontalRoomWrapController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HorizontalRoomWrapController, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/horizontal_room_wrap", 0, 0)

end
