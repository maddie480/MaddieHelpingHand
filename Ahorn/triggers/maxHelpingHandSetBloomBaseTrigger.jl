module MaxHelpingHandSetBloomBaseTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/SetBloomBaseTrigger" SetBloomBaseTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    value::Number=0.0)

const placements = Ahorn.PlacementDict(
    "Set Bloom Base Trigger (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        SetBloomBaseTrigger,
        "rectangle"
    )
)

end
