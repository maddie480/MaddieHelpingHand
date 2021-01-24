module MaxHelpingHandCustomMemorialWithDreamingAttribute

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomMemorialWithDreamingAttribute" CustomMemorialWithDreamingAttribute(x::Integer, y::Integer,
    dreaming::Bool=false, dialog::String="MEMORIAL", sprite::String="scenery/memorial/memorial", spacing::Integer=16)

const placements = Ahorn.PlacementDict(
    "Custom Memorial (Dreaming) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        CustomMemorialWithDreamingAttribute,
        "point",
        Dict{String, Any}(
            "dreaming" => true
        )
    ),
    "Custom Memorial (Not Dreaming) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        CustomMemorialWithDreamingAttribute
    )
)

sprite = "scenery/memorial/memorial"

function Ahorn.selection(entity::CustomMemorialWithDreamingAttribute)
    x, y = Ahorn.position(entity)

    spriteName = get(entity.data, "sprite", sprite)
    customSprite = Ahorn.getSprite(spriteName, "Gameplay")

    if customSprite.width == 0 || customSprite.height == 0
        return Ahorn.Rectangle(x - 4, y - 4, 8, 8)

    else
        return Ahorn.getSpriteRectangle(spriteName, x, y, jx=0.5, jy=1.0)
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomMemorialWithDreamingAttribute, room::Maple.Room)
    spriteName = get(entity.data, "sprite", sprite)

    Ahorn.drawSprite(ctx, spriteName, 0, 0, jx=0.5, jy=1.0)
end

end
