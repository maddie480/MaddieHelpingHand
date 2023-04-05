module MaxHelpingHandFlagToggleSmoothCameraOffsetTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/FlagToggleSmoothCameraOffsetTrigger" FlagToggleSmoothCameraOffsetTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    offsetXFrom::Number=0.0, offsetXTo::Number=0.0, offsetYFrom::Number=0.0, offsetYTo::Number=0.0, positionMode::String="NoEffect", onlyOnce::Bool=false, xOnly::Bool=false, yOnly::Bool=false,
    flag::String="flag_toggle_smooth_camera_offset", inverted::Bool=false)

const placements = Ahorn.PlacementDict(
    "Smooth Camera Offset (Flag-Toggled) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        FlagToggleSmoothCameraOffsetTrigger,
        "rectangle",
    ),
)

function Ahorn.editingOptions(trigger::FlagToggleSmoothCameraOffsetTrigger)
    return Dict{String, Any}(
        "positionMode" => Maple.trigger_position_modes
    )
end

end
