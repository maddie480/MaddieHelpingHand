module MaxHelpingHandSetFlagOnSpawnTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/SetFlagOnSpawnTrigger" SetFlagOnSpawnTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    flags::String="flag_name", enable::Bool=false, ifFlag::String="")

const placements = Ahorn.PlacementDict(
    "Set Flag On Spawn (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        SetFlagOnSpawnTrigger,
        "rectangle"
    )
)

end
