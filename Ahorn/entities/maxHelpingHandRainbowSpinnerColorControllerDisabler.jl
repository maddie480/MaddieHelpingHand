module MaxHelpingHandRainbowSpinnerColorControllerDisabler

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/RainbowSpinnerColorControllerDisabler" RainbowSpinnerColorControllerDisabler(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Rainbow Spinner Color Controller (Disable)\n(Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        RainbowSpinnerColorControllerDisabler
    )
)

function Ahorn.selection(entity::RainbowSpinnerColorControllerDisabler)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RainbowSpinnerColorControllerDisabler, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/rainbowSpinnerColorControllerDisable", 0, 0)

end
