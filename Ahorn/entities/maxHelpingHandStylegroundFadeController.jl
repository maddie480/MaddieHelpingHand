module MaxHelpingHandStylegroundFadeController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/StylegroundFadeController" StylegroundFadeController(x::Integer, y::Integer, flag::String="StylegroundFade", fadeInTime::Number=1.0, fadeOutTime::Number=1.0)

const placements = Ahorn.PlacementDict(
    "Styleground Fade Controller (max480's Helping Hand)" => Ahorn.EntityPlacement(
        StylegroundFadeController
    )
)

function Ahorn.selection(entity::StylegroundFadeController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StylegroundFadeController, room::Maple.Room) = Ahorn.drawImage(ctx, Ahorn.Assets.northernLights, -12, -12)

end
