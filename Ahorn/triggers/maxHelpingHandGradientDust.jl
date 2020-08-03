module MaxHelpingHandGradientDustTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/GradientDustTrigger" GradientDustTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    gradientImage::String="MaxHelpingHand/gradientdustbunnies/bluegreen", scrollSpeed::Number=50.0)

const placements = Ahorn.PlacementDict(
    "Gradient Dust (max480's Helping Hand)" => Ahorn.EntityPlacement(
        GradientDustTrigger,
        "rectangle"
    )
)

end
