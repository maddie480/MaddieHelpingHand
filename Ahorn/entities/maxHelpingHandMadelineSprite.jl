module MaxHelpingHandMadelineSprite

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/MadelineSprite" MadelineSprite(x::Integer, y::Integer, hasBackpack::Bool=true, left::Bool=false, dashCount::Int=1)

const placements = Ahorn.PlacementDict(
    "Madeline Sprite (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        MadelineSprite
    )
)

function getSprite(entity::MadelineSprite)
    if entity.hasBackpack
        return "characters/player/sitDown00"
    else
        return "characters/player_no_backpack/sitDown00"
    end
end

function Ahorn.selection(entity::MadelineSprite)
    x, y = Ahorn.position(entity)

    if entity.left && entity.hasBackpack
        return Ahorn.getSpriteRectangle(getSprite(entity), x, y, jx=0.4, jy=1.0)
    else
        return Ahorn.getSpriteRectangle(getSprite(entity), x, y, jx=0.5, jy=1.0)
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MadelineSprite)
    scaleX = get(entity.data, "left", false) ? -1 : 1
    Ahorn.drawSprite(ctx, getSprite(entity), 0, 0, jx=0.5, jy=1.0, sx=scaleX)
end

end
