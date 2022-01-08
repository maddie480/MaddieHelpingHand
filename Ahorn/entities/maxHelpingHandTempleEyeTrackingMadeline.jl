module MaxHelpingHandTempleEyeTrackingMadeline

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/TempleEyeTrackingMadeline" TempleEyeTrackingMadeline(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Temple Eye (Small, Follow Madeline) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        TempleEyeTrackingMadeline
    )
)

function Ahorn.selection(entity::TempleEyeTrackingMadeline)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y - 6, 12, 12)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TempleEyeTrackingMadeline, room::Maple.Room)
    x, y = Ahorn.position(entity)
    tx, ty = floor(Int, x / 8) + 1, floor(Int, y / 8) + 1

    layer = get(room.fgTiles.data, (ty, tx), '0') == '0' ? "bg" : "fg"

    Ahorn.drawSprite(ctx, "scenery/temple/eye/$(layer)_eye.png", 0, 0)
    Ahorn.drawSprite(ctx, "scenery/temple/eye/$(layer)_lid00.png", 0, 0)
    Ahorn.drawSprite(ctx, "scenery/temple/eye/$(layer)_pupil.png", 0, 0)
end

end
