module MaxHelpingHandReskinnableStarRotateSpinner

using ..Ahorn, Maple

@pardef ReskinnableStarRotateSpinner(x1::Integer, y1::Integer, x2::Integer=x1 + 16, y2::Integer=y1, clockwise::Bool=false, spriteFolder::String="danger/MaxHelpingHand/starSpinner", particleColors::String="EA64B7|3EE852,67DFEA|E85351,EA582C|33BDE8") =
    Entity("MaxHelpingHand/ReskinnableStarRotateSpinner", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], clockwise=clockwise, spriteFolder=spriteFolder, particleColors=particleColors)

function rotatingSpinnerFinalizer(entity::ReskinnableStarRotateSpinner)
    x, y = Int(entity.data["x"]), Int(entity.data["y"])
    entity.data["x"], entity.data["y"] = x + 32, y
    entity.data["nodes"] = [(x, y)]
end

const placements = Ahorn.PlacementDict(
    "Star (Rotating, Reskinnable) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableStarRotateSpinner,
        "point",
        Dict{String, Any}(
            "clockwise" => false
        ),
        rotatingSpinnerFinalizer
    ),
)

Ahorn.nodeLimits(entity::ReskinnableStarRotateSpinner) = 1, 1

function Ahorn.selection(entity::ReskinnableStarRotateSpinner)
    nx, ny = Int.(entity.data["nodes"][1])
    x, y = Ahorn.position(entity)

    return [Ahorn.Rectangle(x - 8, y - 8, 16, 16), Ahorn.Rectangle(nx - 8, ny - 8, 16, 16)]
end

function renderMovingSpinner(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableStarRotateSpinner, x::Number, y::Number)
    spriteFolder = get(entity.data, "spriteFolder", "danger/MaxHelpingHand/starSpinner")

    Ahorn.drawSprite(ctx, "$(spriteFolder)/idle0_00", x, y)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableStarRotateSpinner, room::Maple.Room)
    clockwise = get(entity.data, "clockwise", false)
    dir = clockwise ? 1 : -1

    centerX, centerY = Int.(entity.data["nodes"][1])
    x, y = Ahorn.position(entity)

    radius = sqrt((centerX - x)^2 + (centerY - y)^2)

    Ahorn.drawCircle(ctx, centerX, centerY, radius, Ahorn.colors.selection_selected_fc)
    Ahorn.drawArrow(ctx, centerX + radius, centerY, centerX + radius, centerY + 0.001 * dir, Ahorn.colors.selection_selected_fc, headLength=6)
    Ahorn.drawArrow(ctx, centerX - radius, centerY, centerX - radius, centerY + 0.001 * -dir, Ahorn.colors.selection_selected_fc, headLength=6)

    renderMovingSpinner(ctx, entity, x, y)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableStarRotateSpinner, room::Maple.Room)
    centerX, centerY = Int.(entity.data["nodes"][1])

    renderMovingSpinner(ctx, entity, centerX, centerY)
end

end