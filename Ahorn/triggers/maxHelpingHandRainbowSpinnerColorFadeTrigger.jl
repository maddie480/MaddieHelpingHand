module MaxHelpingHandRainbowSpinnerColorFadeTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/RainbowSpinnerColorFadeTrigger" RainbowSpinnerColorFadeTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    colorsA::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2", gradientSizeA::Number=280.0, loopColorsA::Bool=false, centerXA::Number=0.0, centerYA::Number=0.0, gradientSpeedA::Number=50.0,
    colorsB::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2", gradientSizeB::Number=280.0, loopColorsB::Bool=false, centerXB::Number=0.0, centerYB::Number=0.0, gradientSpeedB::Number=50.0,
    direction::String="NoEffect", persistent::Bool=false)

const placements = Ahorn.PlacementDict(
    "Rainbow Spinner Color Fade (max480's Helping Hand)" => Ahorn.EntityPlacement(
        RainbowSpinnerColorFadeTrigger,
        "rectangle"
    )
)

function Ahorn.editingOptions(trigger::RainbowSpinnerColorFadeTrigger)
    return Dict{String, Any}(
        "direction" => Maple.trigger_position_modes
    )
end

end
