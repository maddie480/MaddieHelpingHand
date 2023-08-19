module MaxHelpingHandReversibleRetentionBooster

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/ReversibleRetentionBooster" ReversibleRetentionBooster(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Reversible Retention Booster (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        ReversibleRetentionBooster
    )
)

function Ahorn.selection(entity::ReversibleRetentionBooster)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle("objects/MaxHelpingHand/reversibleRetentionBooster/booster00", x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReversibleRetentionBooster, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/MaxHelpingHand/reversibleRetentionBooster/booster00", 0, 0)
end

end
