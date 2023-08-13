module MaxHelpingHandFrozenJelly

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/FrozenJelly" FrozenJelly(x::Integer, y::Integer, tutorial::Bool=false,
    spriteDirectory::String="MaxHelpingHand/jellies/frozenjelly", glow::Bool=false)

const placements = Ahorn.PlacementDict(
    "Frozen Jelly (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        FrozenJelly
    )
)

function Ahorn.selection(entity::FrozenJelly)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(entity.spriteDirectory * "/idle0", x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FrozenJelly, room::Maple.Room)
    Ahorn.drawSprite(ctx, entity.spriteDirectory * "/idle0", 0, 0)
end

end
