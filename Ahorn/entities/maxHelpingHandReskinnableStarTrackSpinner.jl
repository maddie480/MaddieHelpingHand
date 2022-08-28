module MaxHelpingHandReskinnableStarTrackSpinner

using ..Ahorn, Maple

@pardef ReskinnableStarTrackSpinner(x1::Integer, y1::Integer, x2::Integer=x1 + 16, y2::Integer=y1, speed::String="Normal", startCenter::Bool=false, spriteFolder::String="danger/MaxHelpingHand/starSpinner", particleColors::String="EA64B7|3EE852,67DFEA|E85351,EA582C|33BDE8", immuneToGuneline::Bool=false) =
    Entity("MaxHelpingHand/ReskinnableStarTrackSpinner", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], speed=speed, startCenter=startCenter, spriteFolder=spriteFolder, particleColors=particleColors, immuneToGuneline=immuneToGuneline)

const placements = Ahorn.PlacementDict(
    "Star (Track, $(uppercasefirst(speed)), Reskinnable) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableStarTrackSpinner,
        "line",
        Dict{String, Any}(
            "speed" => speed
        )
    ) for speed in Maple.track_spinner_speeds
)

Ahorn.editingOptions(entity::ReskinnableStarTrackSpinner) = Dict{String, Any}(
    "speed" => Maple.track_spinner_speeds
)

Ahorn.nodeLimits(entity::ReskinnableStarTrackSpinner) = 1, 1

function Ahorn.selection(entity::ReskinnableStarTrackSpinner)
    startX, startY = Ahorn.position(entity)
    stopX, stopY = Int.(entity.data["nodes"][1])

    return [Ahorn.Rectangle(startX - 8, startY - 8, 16, 16), Ahorn.Rectangle(stopX - 8, stopY - 8, 16, 16)]
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableStarTrackSpinner, room::Maple.Room)
    startX, startY = Ahorn.position(entity)
    stopX, stopY = entity.data["nodes"][1]
    spriteFolder = get(entity.data, "spriteFolder", "danger/MaxHelpingHand/starSpinner")

    Ahorn.drawSprite(ctx, "$(spriteFolder)/idle0_00", stopX, stopY)
    Ahorn.drawArrow(ctx, startX, startY, stopX, stopY, Ahorn.colors.selection_selected_fc, headLength=10)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableStarTrackSpinner, room::Maple.Room)
    startX, startY = Ahorn.position(entity)
    spriteFolder = get(entity.data, "spriteFolder", "danger/MaxHelpingHand/starSpinner")

    Ahorn.drawSprite(ctx, "$(spriteFolder)/idle0_00", startX, startY)
end

end
