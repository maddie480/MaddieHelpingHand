module MaxHelpingHandSidewaysMovingPlatform

using ..Ahorn, Maple

@pardef SidewaysMovingPlatform(x::Integer, y::Integer, height::Integer=Maple.defaultBlockHeight, left::Bool=true, mode::String="Loop", texture::String="default",
    moveTime::Number=2.0, pauseTime::Number=0.0, easing::Bool=true, amount::Int=1, offset::Number=0.0, flag::String="", emitSound::Bool=true, drawTracks::Bool=true,
    accurateTiming::Bool=false) =
    Entity("MaxHelpingHand/SidewaysMovingPlatform", x=x, y=y, nodes=Tuple{Int, Int}[], height=height, left=left, mode=mode, texture=texture,
    moveTime=moveTime, pauseTime=pauseTime, easing=easing, amount=amount, offset=offset, flag=flag, emitSound=emitSound, drawTracks=drawTracks,
    accurateTiming=accurateTiming)

const placements = Ahorn.PlacementDict()

const modes = ["Loop", "LoopNoPause", "BackAndForth", "BackAndForthNoPause", "TeleportBack"]

for texture in Maple.wood_platform_textures
    placements["Sideways Moving Platform ($(uppercasefirst(texture))) (Maddie's Helping Hand)"] = Ahorn.EntityPlacement(
        SidewaysMovingPlatform,
        "rectangle",
        Dict{String, Any}(
            "texture" => texture
        ),
        function(entity)
            x, y = Int(entity.data["x"]), Int(entity.data["y"])
            entity.data["x"], entity.data["y"] = x + 16, y
            entity.data["nodes"] = [(x, y)]
        end
    )
end

Ahorn.editingOptions(entity::SidewaysMovingPlatform) = Dict{String, Any}(
    "texture" => Maple.wood_platform_textures,
    "mode" => modes
)

Ahorn.nodeLimits(entity::SidewaysMovingPlatform) = 1, -1

Ahorn.resizable(entity::SidewaysMovingPlatform) = false, true

Ahorn.minimumSize(entity::SidewaysMovingPlatform) = 0, 8

function Ahorn.selection(entity::SidewaysMovingPlatform)
    height = Int(get(entity.data, "height", 8))

    nodes = get(entity.data, "nodes", ())
    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    rectangles = Ahorn.Rectangle[Ahorn.Rectangle(startX, startY, 8, height)]

    for node in nodes
        nodeX, nodeY = Int.(node)
        push!(rectangles, Ahorn.Rectangle(nodeX, nodeY, 8, height))
    end

    return rectangles
end

outerColor = (30, 14, 25) ./ 255
innerColor = (10, 0, 6) ./ 255

function renderConnection(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, nx::Number, ny::Number, height::Number)
    cx, cy = x + 4, y + floor(Int, height / 2)
    cnx, cny = nx + 4, ny + floor(Int, height / 2)

    length = sqrt((x - nx)^2 + (y - ny)^2)
    theta = atan(cny - cy, cnx - cx)

    Ahorn.Cairo.save(ctx)

    Ahorn.translate(ctx, cx, cy)
    Ahorn.rotate(ctx, theta)

    Ahorn.setSourceColor(ctx, outerColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 3);

    Ahorn.move_to(ctx, 0, 0)
    Ahorn.line_to(ctx, length, 0)

    Ahorn.stroke(ctx)

    Ahorn.setSourceColor(ctx, innerColor)
    Ahorn.set_antialias(ctx, 1)
    Ahorn.set_line_width(ctx, 1);

    Ahorn.move_to(ctx, 0, 0)
    Ahorn.line_to(ctx, length, 0)

    Ahorn.stroke(ctx)

    Ahorn.Cairo.restore(ctx)
end

function renderPlatform(ctx::Ahorn.Cairo.CairoContext, texture::String, x::Number, y::Number, height::Number, left::Bool)
    tilesHeight = div(height, 8)

    Ahorn.Cairo.save(ctx)
    Ahorn.translate(ctx, x, y)
    Ahorn.rotate(ctx, left ? - pi / 2 : pi / 2)

    offsetX = left ? -height : 0
    offsetY = left ? 0 : -8
    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", 8 * (i - 1) + offsetX, offsetY, 8, 0, 8, 8)
    end

    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", offsetX, offsetY, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", tilesHeight * 8 - 8 + offsetX, offsetY, 24, 0, 8, 8)
    Ahorn.drawImage(ctx, "objects/woodPlatform/$texture", floor(Int, height / 2) - 4 + offsetX, offsetY, 16, 0, 8, 8)

    Ahorn.Cairo.restore(ctx)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::SidewaysMovingPlatform, room::Maple.Room)
    height = Int(get(entity.data, "height", 8))
    mode = get(entity.data, "mode", "Loop")
    left = get(entity.data, "left", true)

    firstNodeX, firstNodeY = Int(entity.data["x"]), Int(entity.data["y"])
    previousNodeX, previousNodeY = firstNodeX, firstNodeY

    texture = get(entity.data, "texture", "default")

    nodes = get(entity.data, "nodes", ())
    for node in nodes
        nodeX, nodeY = Int.(node)

        if get(entity, "drawTracks", true)
            renderConnection(ctx, previousNodeX, previousNodeY, nodeX, nodeY, height)
        end

        previousNodeX, previousNodeY = nodeX, nodeY
    end

    if (mode == "Loop" || mode == "LoopNoPause") && get(entity, "drawTracks", true)
        renderConnection(ctx, previousNodeX, previousNodeY, firstNodeX, firstNodeY, height)
    end

    renderPlatform(ctx, texture, firstNodeX, firstNodeY, height, left)
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::SidewaysMovingPlatform, room::Maple.Room)
    height = Int(get(entity.data, "height", 8))
    mode = get(entity.data, "mode", "Loop")
    left = get(entity.data, "left", true)

    firstNodeX, firstNodeY = Int(entity.data["x"]), Int(entity.data["y"])
    previousNodeX, previousNodeY = firstNodeX, firstNodeY

    texture = get(entity.data, "texture", "default")

    nodes = get(entity.data, "nodes", ())
    for node in nodes
        nodeX, nodeY = Int.(node)
        renderPlatform(ctx, texture, nodeX, nodeY, height, left)
        Ahorn.drawArrow(ctx, previousNodeX + 4, previousNodeY + height / 2, nodeX + 4, nodeY + height / 2, Ahorn.colors.selection_selected_fc, headLength=6)
        previousNodeX, previousNodeY = nodeX, nodeY
    end

    if mode == "Loop" || mode == "LoopNoPause"
        Ahorn.drawArrow(ctx, previousNodeX + 4, previousNodeY + height / 2, firstNodeX + 4, firstNodeY + height / 2, Ahorn.colors.selection_selected_fc, headLength=6)
    end
end

end
