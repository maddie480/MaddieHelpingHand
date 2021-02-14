module MaxHelpingHandSeekerBarrierColorControllerDisabler

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/SeekerBarrierColorControllerDisabler" SeekerBarrierColorControllerDisabler(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Seeker Barrier Color Controller (Disable)\n(max480's Helping Hand)" => Ahorn.EntityPlacement(
        SeekerBarrierColorControllerDisabler
    )
)

function Ahorn.selection(entity::SeekerBarrierColorControllerDisabler)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SeekerBarrierColorControllerDisabler, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/rainbowSpinnerColorControllerDisable", 0, 0)

end
