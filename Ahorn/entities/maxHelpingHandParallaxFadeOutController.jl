module MaxHelpingHandParallaxFadeOutController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/ParallaxFadeOutController" ParallaxFadeOutController(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Parallax Fade Out Controller (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ParallaxFadeOutController
    )
)

function Ahorn.selection(entity::ParallaxFadeOutController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ParallaxFadeOutController, room::Maple.Room) = Ahorn.drawImage(ctx, Ahorn.Assets.northernLights, -12, -12)

end
