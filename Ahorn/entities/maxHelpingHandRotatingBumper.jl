module MaxHelpingHandRotatingBumper

using ..Ahorn, Maple

@pardef RotatingBumper(x1::Integer, y1::Integer, x2::Integer=x1 + 16, y2::Integer=y1, speed::Number=360.0, attachToCenter::Bool=false, notCoreMode::Bool=false) =
    Entity("MaxHelpingHand/RotatingBumper", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], speed=speed, attachToCenter=attachToCenter, notCoreMode=notCoreMode)
    
const placements = Ahorn.PlacementDict(
    "Bumper (Rotating) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        RotatingBumper,
        "point",
        Dict{String, Any}(),
        function(entity::RotatingBumper)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            entity.data["nodes"] = Tuple{Int, Int}[(x + 32, y)]
        end
    )
)

Ahorn.nodeLimits(entity::RotatingBumper) = 1, 1

sprite = "objects/Bumper/Idle22.png"

function Ahorn.selection(entity::RotatingBumper)
    nx, ny = Int.(entity.data["nodes"][1])
    x, y = Ahorn.position(entity)

    return [Ahorn.getSpriteRectangle(sprite, x, y), Ahorn.getSpriteRectangle(sprite, nx, ny)]
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::RotatingBumper, room::Maple.Room)
    x, y = Ahorn.position(entity)
    
    Ahorn.drawSprite(ctx, sprite, x, y)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::RotatingBumper, room::Maple.Room)
    speed = get(entity.data, "speed", 360.0)
    dir = speed > 0 ? 1 : -1

    centerX, centerY = Int.(entity.data["nodes"][1])
    x, y = Ahorn.position(entity)

    radius = sqrt((centerX - x)^2 + (centerY - y)^2)

    Ahorn.drawCircle(ctx, centerX, centerY, radius, Ahorn.colors.selection_selected_fc)
    Ahorn.drawArrow(ctx, centerX + radius, centerY, centerX + radius, centerY + 0.001 * dir, Ahorn.colors.selection_selected_fc, headLength=6)
    Ahorn.drawArrow(ctx, centerX - radius, centerY, centerX - radius, centerY + 0.001 * -dir, Ahorn.colors.selection_selected_fc, headLength=6)
end

end
