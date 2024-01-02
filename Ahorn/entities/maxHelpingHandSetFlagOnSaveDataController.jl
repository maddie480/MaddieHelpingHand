module MaxHelpingHandSetFlagOnSaveDataController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/SetFlagOnHeartCollectedController" SetFlagOnHeartCollectedController(x::Integer, y::Integer, flag::String="flag_name")
@mapdef Entity "MaxHelpingHand/SetFlagOnCompletionController" SetFlagOnCompletionController(x::Integer, y::Integer, flag::String="flag_name")
@mapdef Entity "MaxHelpingHand/SetFlagOnFullClearController" SetFlagOnFullClearController(x::Integer, y::Integer, flag::String="flag_name")

const placements = Ahorn.PlacementDict(
    "Set Flag On Heart Collected Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        SetFlagOnHeartCollectedController
    ),
    "Set Flag On Completion Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        SetFlagOnCompletionController
    ),
    "Set Flag On Full Clear Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        SetFlagOnFullClearController
    )
)

const controllersUnion = Union{SetFlagOnHeartCollectedController, SetFlagOnCompletionController, SetFlagOnFullClearController}

function Ahorn.selection(entity::controllersUnion)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::controllersUnion, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/set_flag_on_spawn", 0, 0)

end
