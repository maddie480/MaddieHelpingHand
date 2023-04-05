module MaxHelpingHandPauseBadelineBossesTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/PauseBadelineBossesTrigger" PauseBadelineBossesTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight)

const placements = Ahorn.PlacementDict(
    "Pause Badeline Bosses (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        PauseBadelineBossesTrigger,
        "rectangle",
    ),
)

end
