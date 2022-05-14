module MaxHelpingHandReskinnableFloatingDebris

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/ReskinnableFloatingDebris" ReskinnableFloatingDebris(x::Integer, y::Integer, texture::String="scenery/debris", depth::Int=-5, interactWithPlayer::Bool=true)

const placements = Ahorn.PlacementDict(
    "Floating Debris (Reskinnable) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableFloatingDebris
    )
)

function Ahorn.selection(entity::ReskinnableFloatingDebris)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y - 6, 12, 12)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableFloatingDebris, room::Maple.Room)
    debrisSprite = Ahorn.getSprite(entity.texture, "Gameplay")
    Ahorn.drawImage(ctx, debrisSprite, -4, -4, 0, 0, 8, 8)
end

end
