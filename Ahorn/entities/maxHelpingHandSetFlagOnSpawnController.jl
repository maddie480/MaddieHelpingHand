module MaxHelpingHandSetFlagOnSpawnController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/SetFlagOnSpawnController" SetFlagOnSpawnController(x::Integer, y::Integer, flag::String="flag_name", enable::Bool=false)

const placements = Ahorn.PlacementDict(
    "Set Flag On Spawn Controller (max480's Helping Hand)" => Ahorn.EntityPlacement(
        SetFlagOnSpawnController
    )
)

function Ahorn.selection(entity::SetFlagOnSpawnController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SetFlagOnSpawnController, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/set_flag_on_spawn", 0, 0)

end
