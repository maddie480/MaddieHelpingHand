module MaxHelpingHandMadelinePonytailTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/MadelinePonytailTrigger" MadelinePonytailTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, enable::Bool=true)

const placements = Ahorn.PlacementDict(
    "Madeline Ponytail Trigger (max480's Helping Hand)" => Ahorn.EntityPlacement(
        MadelinePonytailTrigger,
        "rectangle"
    )
)

end
