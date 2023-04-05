module MaxHelpingHandParallaxFadeSpeedController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/ParallaxFadeSpeedController" ParallaxFadeSpeedController(x::Integer, y::Integer, fadeTime::Number=1.0)

const placements = Ahorn.PlacementDict(
    "Parallax Fade Speed Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        ParallaxFadeSpeedController
    )
)

function Ahorn.selection(entity::ParallaxFadeSpeedController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ParallaxFadeSpeedController, room::Maple.Room) = Ahorn.drawImage(ctx, Ahorn.Assets.northernLights, -12, -12)

end
