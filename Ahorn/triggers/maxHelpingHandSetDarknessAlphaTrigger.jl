module MaxHelpingHandSetDarknessAlphaTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/SetDarknessAlphaTrigger" SetDarknessAlphaTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    value::Number=0.0)

const placements = Ahorn.PlacementDict(
    "Set Darkness Alpha Trigger (max480's Helping Hand)" => Ahorn.EntityPlacement(
        SetDarknessAlphaTrigger,
        "rectangle"
    )
)

end
