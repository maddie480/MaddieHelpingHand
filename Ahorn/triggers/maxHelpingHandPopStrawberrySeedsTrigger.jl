module MaxHelpingHandPopStrawberrySeedsTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/PopStrawberrySeedsTrigger" PopStrawberrySeedsTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight)

const placements = Ahorn.PlacementDict(
    "Pop Strawberry Seeds (max480's Helping Hand)" => Ahorn.EntityPlacement(
        PopStrawberrySeedsTrigger,
        "rectangle"
    )
)

end
