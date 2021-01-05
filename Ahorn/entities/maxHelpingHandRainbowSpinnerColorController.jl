module MaxHelpingHandRainbowSpinnerColorController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/RainbowSpinnerColorController" RainbowSpinnerColorController(x::Integer, y::Integer, colors::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2",
    gradientSize::Number=280.0, loopColors::Bool=false, centerX::Number=0.0, centerY::Number=0.0, gradientSpeed::Number=50.0, persistent::Bool=false)

const placements = Ahorn.PlacementDict(
    "Rainbow Spinner Color Controller (max480's Helping Hand)" => Ahorn.EntityPlacement(
        RainbowSpinnerColorController
    )
)

function Ahorn.selection(entity::RainbowSpinnerColorController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RainbowSpinnerColorController, room::Maple.Room) = Ahorn.drawImage(ctx, Ahorn.Assets.northernLights, -12, -12)

end
