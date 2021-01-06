module MaxHelpingHandAmbienceVolumeTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/AmbienceVolumeTrigger" AmbienceVolumeTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    from::Number=0.0, to::Number=0.0, direction::String="NoEffect")

const placements = Ahorn.PlacementDict(
    "Ambience Volume (max480's Helping Hand)" => Ahorn.EntityPlacement(
        AmbienceVolumeTrigger,
        "rectangle"
    )
)

function Ahorn.editingOptions(trigger::AmbienceVolumeTrigger)
    return Dict{String, Any}(
        "direction" => Maple.trigger_position_modes
    )
end

end
