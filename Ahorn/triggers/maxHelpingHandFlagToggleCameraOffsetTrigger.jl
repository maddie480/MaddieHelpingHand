module MaxHelpingHandFlagToggleCameraOffsetTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/FlagToggleCameraOffsetTrigger" FlagToggleCameraOffsetTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    cameraX::Number=0.0, cameraY::Number=0.0, flag::String="flag_toggle_camera_offset", inverted::Bool=false)

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Camera Offset (Flag-Toggled) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        FlagToggleCameraOffsetTrigger,
        "rectangle"
    )
)

end
