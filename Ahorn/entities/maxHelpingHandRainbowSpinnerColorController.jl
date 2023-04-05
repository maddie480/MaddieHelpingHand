module MaxHelpingHandRainbowSpinnerColorController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/RainbowSpinnerColorController" RainbowSpinnerColorController(x::Integer, y::Integer, colors::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2",
    gradientSize::Number=280.0, loopColors::Bool=false, centerX::Number=0.0, centerY::Number=0.0, gradientSpeed::Number=50.0, persistent::Bool=false)
@mapdef Entity "MaxHelpingHand/FlagRainbowSpinnerColorController" FlagRainbowSpinnerColorController(x::Integer, y::Integer, colors::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2",
    gradientSize::Number=280.0, loopColors::Bool=false, centerX::Number=0.0, centerY::Number=0.0, gradientSpeed::Number=50.0, persistent::Bool=false, flag::String="my_flag",
    colorsWithFlag::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2", gradientSizeWithFlag::Number=280.0, loopColorsWithFlag::Bool=false, centerXWithFlag::Number=0.0, centerYWithFlag::Number=0.0, gradientSpeedWithFlag::Number=50.0)

const placements = Ahorn.PlacementDict(
    "Rainbow Spinner Color Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        RainbowSpinnerColorController
    ),
    "Flag Rainbow Spinner Color Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        FlagRainbowSpinnerColorController
    ),
)

Ahorn.editingOrder(entity::FlagRainbowSpinnerColorController) = String["x", "y", "centerX", "centerXWithFlag", "centerY", "centerYWithFlag", "colors", "colorsWithFlag", "gradientSize", "gradientSizeWithFlag", "gradientSpeed", "gradientSpeedWithFlag"]

const union = Union{RainbowSpinnerColorController, FlagRainbowSpinnerColorController}

function Ahorn.selection(entity::union)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::union, room::Maple.Room) = Ahorn.drawImage(ctx, Ahorn.Assets.northernLights, -12, -12)

end
