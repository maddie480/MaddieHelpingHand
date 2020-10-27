module MaxHelpingHandSpeedBasedMusicParamTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/SpeedBasedMusicParamTrigger" SpeedBasedMusicParamTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    paramName::String="fade", minSpeed::Number=0.0, maxSpeed::Number=90.0, minParamValue::Number=0.0, maxParamValue::Number=1.0, activate::Bool=true)

const placements = Ahorn.PlacementDict(
    "Speed-Based Music Param (max480's Helping Hand)" => Ahorn.EntityPlacement(
        SpeedBasedMusicParamTrigger,
        "rectangle"
    )
)

Ahorn.editingOrder(entity::SpeedBasedMusicParamTrigger) = String["x", "y", "width", "height", "minSpeed", "maxSpeed", "minParamValue", "maxParamValue"]

end
