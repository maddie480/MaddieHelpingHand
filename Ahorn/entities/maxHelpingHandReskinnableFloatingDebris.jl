module MaxHelpingHandReskinnableFloatingDebris

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/ReskinnableFloatingDebris" ReskinnableFloatingDebris(x::Integer, y::Integer, texture::String="scenery/debris", depth::Int=-5,
    interactWithPlayer::Bool=true, debrisWidth::Int=8, debrisHeight::Int=8, rotateSpeed::String="")

const placements = Ahorn.PlacementDict(
    "Floating Debris (Reskinnable) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableFloatingDebris
    )
)

function Ahorn.selection(entity::ReskinnableFloatingDebris)
    x, y = Ahorn.position(entity)

    debrisWidth = get(entity, "debrisWidth", 8)
    debrisHeight = get(entity, "debrisHeight", 8)
    return Ahorn.Rectangle(x - (debrisWidth / 2) - 2, y - (debrisHeight / 2) - 2, debrisWidth + 4, debrisHeight + 4)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableFloatingDebris, room::Maple.Room)
    debrisSprite = Ahorn.getSprite(entity.texture, "Gameplay")
    debrisWidth = get(entity, "debrisWidth", 8)
    debrisHeight = get(entity, "debrisHeight", 8)
    Ahorn.drawImage(ctx, debrisSprite, -(debrisWidth / 2), -(debrisHeight / 2), 0, 0, debrisWidth, debrisHeight)
end

end
