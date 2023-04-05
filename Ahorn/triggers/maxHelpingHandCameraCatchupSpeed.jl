module MaxHelpingHandCameraCatchupSpeedTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/CameraCatchupSpeedTrigger" CameraCatchupSpeedTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    catchupSpeed::Number=1.0, revertOnLeave::Bool=true)

const placements = Ahorn.PlacementDict(
    "Camera Catchup Speed (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CameraCatchupSpeedTrigger,
        "rectangle"
    )
)

end