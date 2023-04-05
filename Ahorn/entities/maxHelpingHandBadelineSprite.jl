module MaxHelpingHandBadelineSprite

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/BadelineSprite" BadelineSprite(x::Integer, y::Integer, left::Bool=false, floating::Bool=false)

const placements = Ahorn.PlacementDict(
    "Badeline Sprite (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        BadelineSprite
    )
)

function Ahorn.selection(entity::BadelineSprite)
    x, y = Ahorn.position(entity)

    scaleX = get(entity.data, "left", false) ? -1 : 1

    spriteName = "characters/badeline/idle00"
    sprite = Ahorn.getSprite(spriteName, "Gameplay")

    return Ahorn.getSpriteRectangle(spriteName, x, y, jx=0.5, jy=1.0, sx=scaleX)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BadelineSprite, room::Maple.Room)
    scaleX = get(entity.data, "left", false) ? -1 : 1
    spriteName = "characters/badeline/idle00"

    Ahorn.drawSprite(ctx, spriteName, 0, 0, jx=0.5, jy=1.0, sx=scaleX)
end

end
