module MaxHelpingHandOneWayCameraTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/OneWayCameraTrigger" OneWayCameraTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    left::Bool=true, right::Bool=true, up::Bool=true, down::Bool=true, flag::String="", blockPlayer::Bool=false)

const placements = Ahorn.PlacementDict(
    "One-Way Camera Trigger (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        OneWayCameraTrigger,
        "rectangle"
    )
)

end
