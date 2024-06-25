module MaxHelpingHandComment

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/Comment" Comment(x::Integer, y::Integer, width::Integer=16, height::Integer=16, comment::String="",
    displayOnMap::Bool=true, textColor::String="FFFFFF")

const placements = Ahorn.PlacementDict(
    "Comment (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        Comment
    )
)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Comment, room::Maple.Room)
    if entity.displayOnMap && entity.comment != ""
        color = Ahorn.argb32ToRGBATuple(parse(Int, get(entity, "textColor", "FFFFFF"), base=16)) ./ 255
        Ahorn.drawCenteredText(ctx, entity.comment, 0, 0, entity.width, entity.height; tint=(color[1], color[2], color[3], 1.0))
    else
        Ahorn.drawImage(ctx, "ahorn/MaxHelpingHand/comment", -12, -12)
    end
end

function Ahorn.selection(entity::Comment)
    x, y = Ahorn.position(entity)

    if entity.displayOnMap && entity.comment != ""
        # text zone is resizable
        return Ahorn.Rectangle(x, y, entity.width, entity.height)
    else
        # size is fixed at 24x24 (the size of the bubble sprite)
        return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
    end
end

function Ahorn.resizable(entity::Comment)
    if entity.displayOnMap && entity.comment != ""
        # text zone is resizable
        return true, true
    else
        # size is fixed at 24x24 (the size of the bubble sprite)
        return false, false
    end
end

end
