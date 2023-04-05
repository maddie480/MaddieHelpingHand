module MaxHelpingHandPopStrawberrySeedsTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/PopStrawberrySeedsTrigger" PopStrawberrySeedsTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    popMultiRoomStrawberrySeeds::Bool=false)

const placements = Ahorn.PlacementDict(
    "Pop Strawberry Seeds (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        PopStrawberrySeedsTrigger,
        "rectangle"
    )
)

end
