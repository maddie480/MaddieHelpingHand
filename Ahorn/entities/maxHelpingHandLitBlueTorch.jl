module MaxHelpingHandLitBlueTorch

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/LitBlueTorch" LitBlueTorch(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Torch (Lit, Blue) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        LitBlueTorch
    )
)

sprite = "objects/temple/torch03"

function Ahorn.selection(entity::LitBlueTorch)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LitBlueTorch, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end