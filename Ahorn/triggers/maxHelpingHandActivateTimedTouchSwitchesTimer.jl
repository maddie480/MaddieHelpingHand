module MaxHelpingHandActivateTimedTouchSwitchesTimerTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/ActivateTimedTouchSwitchesTimerTrigger" ActivateTimedTouchSwitchesTimerTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight)

const placements = Ahorn.PlacementDict(
    "Activate Timed Touch Switches Timer\n(Maddie's Helping Hand + Outback Helper)" => Ahorn.EntityPlacement(
        ActivateTimedTouchSwitchesTimerTrigger,
        "rectangle"
    )
)

end
