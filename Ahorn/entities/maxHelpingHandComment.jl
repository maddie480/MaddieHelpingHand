module MaxHelpingHandComment

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/Comment" Comment(x::Integer, y::Integer, width::Integer=16, height::Integer=16, comment::String="", displayOnMap::Bool=true)

const placements = Ahorn.PlacementDict(
    "Comment (max480's Helping Hand)" => Ahorn.EntityPlacement(
        Comment
    )
)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Comment, room::Maple.Room)
    if entity.displayOnMap && entity.comment != ""
        Ahorn.drawCenteredText(ctx, entity.comment, entity.x - 8, entity.y - 8, entity.width, entity.height; tint=(1.0, 1.0, 1.0, 1.0))
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
