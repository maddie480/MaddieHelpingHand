module MaxHelpingHandFlagToggleCameraCatchupSpeedTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/FlagToggleCameraCatchupSpeedTrigger" FlagToggleCameraCatchupSpeedTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    catchupSpeed::Number=1.0, revertOnLeave::Bool=true, flag::String="flag_toggle_camera_catchup_speed", inverted::Bool=false)

const placements = Ahorn.PlacementDict(
    "Camera Catchup Speed (Flag-Toggled) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        FlagToggleCameraCatchupSpeedTrigger,
        "rectangle"
    )
)

end