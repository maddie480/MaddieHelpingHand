module MaxHelpingHandRainbowSpinnerColorAreaController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/RainbowSpinnerColorAreaController" RainbowSpinnerColorAreaController(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    colors::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2", gradientSize::Number=280.0, loopColors::Bool=false, centerX::Number=0.0, centerY::Number=0.0, gradientSpeed::Number=50.0)
@mapdef Entity "MaxHelpingHand/FlagRainbowSpinnerColorAreaController" FlagRainbowSpinnerColorAreaController(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    colors::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2", gradientSize::Number=280.0, loopColors::Bool=false, centerX::Number=0.0, centerY::Number=0.0, gradientSpeed::Number=50.0, flag::String="my_flag",
    colorsWithFlag::String="89E5AE,88E0E0,87A9DD,9887DB,D088E2", gradientSizeWithFlag::Number=280.0, loopColorsWithFlag::Bool=false, centerXWithFlag::Number=0.0, centerYWithFlag::Number=0.0, gradientSpeedWithFlag::Number=50.0)

const placements = Ahorn.PlacementDict(
    "Rainbow Spinner Color Area Controller\n(Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        RainbowSpinnerColorAreaController,
        "rectangle"
    ),
    "Flag Rainbow Spinner Color Area Controller\n(Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        FlagRainbowSpinnerColorAreaController,
        "rectangle"
    )
)

Ahorn.editingOrder(entity::FlagRainbowSpinnerColorAreaController) = String["x", "y", "width", "height", "centerX", "centerXWithFlag", "centerY", "centerYWithFlag", "colors", "colorsWithFlag", "gradientSize", "gradientSizeWithFlag", "gradientSpeed", "gradientSpeedWithFlag"]

const union = Union{RainbowSpinnerColorAreaController, FlagRainbowSpinnerColorAreaController}

Ahorn.minimumSize(entity::union) = 8, 8
Ahorn.resizable(entity::union) = true, true

Ahorn.selection(entity::union) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::union, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.4, 0.4, 1.0, 0.4), (0.4, 0.4, 1.0, 1.0))
end

end
