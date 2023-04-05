module MaxHelpingHandSetBloomStrengthTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/SetBloomStrengthTrigger" SetBloomStrengthTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    value::Number=1.0)

const placements = Ahorn.PlacementDict(
    "Set Bloom Strength Trigger (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        SetBloomStrengthTrigger,
        "rectangle"
    )
)

end
