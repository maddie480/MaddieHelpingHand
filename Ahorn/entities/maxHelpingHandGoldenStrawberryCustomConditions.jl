module MaxHelpingHandGoldenStrawberryCustomConditions

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/GoldenStrawberryCustomConditions" GoldenStrawberryCustomConditions(x::Integer, y::Integer,
    mustNotDieAndVisitFurtherRooms::Bool=true, mustHaveUnlockedCSides::Bool=true, mustHaveCompletedLevel::Bool=true, showGoldenChapterCard::Bool=true)

const placements = Ahorn.PlacementDict(
    "Golden Strawberry (Custom Conditions)\n(max480's Helping Hand)" => Ahorn.EntityPlacement(
        GoldenStrawberryCustomConditions
    )
)

sprite = "collectables/goldberry/idle00"

function Ahorn.selection(entity::GoldenStrawberryCustomConditions)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GoldenStrawberryCustomConditions, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end
