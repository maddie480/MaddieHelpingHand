module MaxHelpingHandFloatFadeTriggers

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/BloomBaseFadeTrigger" BloomBaseFadeTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    fadeA::Number=0.0, fadeB::Number=1.0, positionMode::String="LeftToRight")
@mapdef Trigger "MaxHelpingHand/BloomStrengthFadeTrigger" BloomStrengthFadeTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    fadeA::Number=0.0, fadeB::Number=1.0, positionMode::String="LeftToRight")
@mapdef Trigger "MaxHelpingHand/DarknessAlphaFadeTrigger" DarknessAlphaFadeTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    fadeA::Number=0.0, fadeB::Number=1.0, positionMode::String="LeftToRight")

const placements = Ahorn.PlacementDict(
    "Bloom Base Fade (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        BloomBaseFadeTrigger,
        "rectangle"
    ),
    "Bloom Strength Fade (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        BloomStrengthFadeTrigger,
        "rectangle"
    ),
    "Darkness Alpha Fade (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        DarknessAlphaFadeTrigger,
        "rectangle"
    )
)

function Ahorn.editingOptions(trigger::Union{BloomBaseFadeTrigger, BloomStrengthFadeTrigger, DarknessAlphaFadeTrigger})
    return Dict{String, Any}(
        "positionMode" => Maple.trigger_position_modes
    )
end

end
