module MaxHelpingHandRainbowSpinnerColorTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/RainbowSpinnerColorTrigger" RainbowSpinnerColorTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    colors::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2", gradientSize::Number=280.0, loopColors::Bool=false, centerX::Number=0.0, centerY::Number=0.0, gradientSpeed::Number=50.0, persistent::Bool=false)

const placements = Ahorn.PlacementDict(
    "Rainbow Spinner Color (max480's Helping Hand)" => Ahorn.EntityPlacement(
        RainbowSpinnerColorTrigger,
        "rectangle"
    )
)

end
