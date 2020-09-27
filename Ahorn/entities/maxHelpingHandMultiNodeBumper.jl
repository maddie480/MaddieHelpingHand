module MaxHelpingHandMultiNodeBumper

using ..Ahorn, Maple

@pardef MultiNodeBumper(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, mode::String="Loop", moveTime::Number=2.0, pauseTime::Number=0.0, easing::Bool=true) =
    Entity("MaxHelpingHand/MultiNodeBumper", x=x, y=y, nodes=Tuple{Int, Int}[], width=width, mode=mode, moveTime=moveTime, pauseTime=pauseTime, easing=easing)
    
const placements = Ahorn.PlacementDict(
    "Bumper (Multi-Node) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        MultiNodeBumper,
        "point",
        Dict{String, Any}(),
        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            entity.data["x"], entity.data["y"] = x + 32, y
            entity.data["nodes"] = [(x, y)]
        end
    )
)

Ahorn.nodeLimits(entity::MultiNodeBumper) = 1, -1

Ahorn.editingOptions(entity::MultiNodeBumper) = Dict{String, Any}(
    "mode" => ["Loop", "LoopNoPause", "BackAndForth", "BackAndForthNoPause", "TeleportBack"]
)

sprite = "objects/Bumper/Idle22.png"

function Ahorn.selection(entity::MultiNodeBumper)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())
    rectangles = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    
    for node in nodes
        nx, ny = Int.(node)
        push!(rectangles, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return rectangles
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::MultiNodeBumper)
    x, y = Ahorn.position(entity)
    nodes = get(entity.data, "nodes", ())
    mode = get(entity.data, "mode", "Loop")
    previousNodeX, previousNodeY = x, y
    
    for node in nodes
        nx, ny = Int.(node)

        theta = atan(previousNodeY - ny, previousNodeX - nx)
        Ahorn.drawSprite(ctx, sprite, nx, ny)
        Ahorn.drawArrow(ctx, previousNodeX, previousNodeY, nx + cos(theta) * 8, ny + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=6)
        previousNodeX, previousNodeY = nx, ny
    end
    
    if mode == "Loop" || mode == "LoopNoPause"
        theta = atan(previousNodeY - y, previousNodeX - x)
        Ahorn.drawArrow(ctx, previousNodeX, previousNodeY, x + cos(theta) * 8, y + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MultiNodeBumper, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end