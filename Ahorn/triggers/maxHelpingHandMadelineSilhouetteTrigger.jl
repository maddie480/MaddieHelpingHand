module MaxHelpingHandMadelineSilhouetteTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/MadelineSilhouetteTrigger" MadelineSilhouetteTrigger(x::Integer, y::Integer, 
    width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, enable::Bool=true)

const placements = Ahorn.PlacementDict(
    "Madeline Silhouette (max480's Helping Hand)" => Ahorn.EntityPlacement(
        MadelineSilhouetteTrigger,
        "rectangle"
    )
)

end
