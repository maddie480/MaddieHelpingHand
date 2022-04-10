module MaxHelpingHandInstantLavaBlockerTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/InstantLavaBlockerTrigger" InstantLavaBlockerTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    canReenter::Bool=false)

const placements = Ahorn.PlacementDict(
    "Instant Lava Blocker (Everest + max480's Helping Hand)" => Ahorn.EntityPlacement(
        InstantLavaBlockerTrigger,
        "rectangle"
    )
)

end
