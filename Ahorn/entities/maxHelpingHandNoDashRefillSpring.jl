module MaxHelpingHandNoDashRefillSpring

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/NoDashRefillSpring" NoDashRefillSpring(x::Integer, y::Integer, spriteDirectory::String="objects/MaxHelpingHand/noDashRefillSpring", playerCanUse::Bool=true, ignoreLighting::Bool=false)
@mapdef Entity "MaxHelpingHand/NoDashRefillSpringRight" NoDashRefillSpringRight(x::Integer, y::Integer, spriteDirectory::String="objects/MaxHelpingHand/noDashRefillSpring", ignoreLighting::Bool=false)
@mapdef Entity "MaxHelpingHand/NoDashRefillSpringLeft" NoDashRefillSpringLeft(x::Integer, y::Integer, spriteDirectory::String="objects/MaxHelpingHand/noDashRefillSpring", ignoreLighting::Bool=false)

const placements = Ahorn.PlacementDict(
    "No Dash Refill Spring (Up) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        NoDashRefillSpring
    ),
    "No Dash Refill Spring (Left) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        NoDashRefillSpringRight
    ),
    "No Dash Refill Spring (Right) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        NoDashRefillSpringLeft
    ),
)

function Ahorn.selection(entity::NoDashRefillSpring)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y - 3, 12, 5)
end

function Ahorn.selection(entity::NoDashRefillSpringLeft)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 1, y - 6, 5, 12)
end

function Ahorn.selection(entity::NoDashRefillSpringRight)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 4, y - 6, 5, 12)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NoDashRefillSpring, room::Maple.Room) = Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "objects/MaxHelpingHand/noDashRefillSpring") * "/00.png", 0, -8)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NoDashRefillSpringLeft, room::Maple.Room) = Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "objects/MaxHelpingHand/noDashRefillSpring") * "/00.png", 24, 0, rot=pi / 2)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NoDashRefillSpringRight, room::Maple.Room) = Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "objects/MaxHelpingHand/noDashRefillSpring") * "/00.png", -8, 16, rot=-pi / 2)

end
