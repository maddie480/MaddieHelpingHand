module MaxHelpingHandReskinnableSwapBlock

using ..Ahorn, Maple

@pardef ReskinnableSwapBlock(x1::Integer, y1::Integer, x2::Integer=x1+16, y2::Integer=y1, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, spriteDirectory::String="objects/swapblock") =
    Entity("MaxHelpingHand/ReskinnableSwapBlock", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], width=width, height=height, spriteDirectory=spriteDirectory)
    
function swapFinalizer(entity)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    entity.data["nodes"] = [(x + width, y)]
end

const placements = Ahorn.PlacementDict(
    "Reskinnable Swap Block (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableSwapBlock,
        "rectangle",
        Dict{String, Any}(),
        swapFinalizer
    )
)

Ahorn.editingOptions(entity::ReskinnableSwapBlock) = Dict{String, Any}(
    "spriteDirectory" => String["objects/swapblock", "objects/swapblock/moon"]
)

Ahorn.nodeLimits(entity::ReskinnableSwapBlock) = 1, 1

Ahorn.minimumSize(entity::ReskinnableSwapBlock) = 16, 16
Ahorn.resizable(entity::ReskinnableSwapBlock) = true, true

function Ahorn.selection(entity::ReskinnableSwapBlock)
    x, y = Ahorn.position(entity)
    stopX, stopY = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(stopX, stopY, width, height)]
end

function getTextures(entity::ReskinnableSwapBlock)
    spriteDirectory = get(entity, "spriteDirectory", "objects/swapblock")
    return "$(spriteDirectory)/blockRed", "$(spriteDirectory)/target", "$(spriteDirectory)/midBlockRed00"
end

function renderTrail(ctx, x::Number, y::Number, width::Number, height::Number, trail::String)
    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    offset = (trail == "objects/swapblock/target" || trail == "objects/swapblock/moon/target" ? 0 : 2)

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, trail, x + (i - 1) * 8, y + 2, 6 + offset, 0 + offset, 8, 6)
        Ahorn.drawImage(ctx, trail, x + (i - 1) * 8, y + height - 8, 6 + offset, 14 + offset, 8, 6)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, trail, x + 2, y + (i - 1) * 8, 0 + offset, 6 + offset, 6, 8)
        Ahorn.drawImage(ctx, trail, x + width - 8, y + (i - 1) * 8, 14 + offset, 6 + offset, 6, 8)
    end

    for i in 2:tilesWidth - 1, j in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, trail, x + (i - 1) * 8, y + (j - 1) * 8, 6 + offset, 6 + offset, 8, 8)
    end

    Ahorn.drawImage(ctx, trail, x + width - 8, y + 2, 14 + offset, 0 + offset, 6, 6)
    Ahorn.drawImage(ctx, trail, x + width - 8, y + height - 8, 14 + offset, 14 + offset, 6, 6)
    Ahorn.drawImage(ctx, trail, x + 2, y + 2, 0 + offset, 0 + offset, 6, 6)
    Ahorn.drawImage(ctx, trail, x + 2, y + height - 8, 0 + offset, 14 + offset, 6, 6)
end

function renderSwapBlock(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, width::Number, height::Number, midResource::String, frame::String)
    midSprite = Ahorn.getSprite(midResource, "Gameplay")
    
    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, frame, x + (i - 1) * 8, y, 8, 0, 8, 8)
        Ahorn.drawImage(ctx, frame, x + (i - 1) * 8, y + height - 8, 8, 16, 8, 8)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, frame, x, y + (i - 1) * 8, 0, 8, 8, 8)
        Ahorn.drawImage(ctx, frame, x + width - 8, y + (i - 1) * 8, 16, 8, 8, 8)
    end

    for i in 2:tilesWidth - 1, j in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, frame, x + (i - 1) * 8, y + (j - 1) * 8, 8, 8, 8, 8)
    end

    Ahorn.drawImage(ctx, frame, x, y, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, x + width - 8, y, 16, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, x, y + height - 8, 0, 16, 8, 8)
    Ahorn.drawImage(ctx, frame, x + width - 8, y + height - 8, 16, 16, 8, 8)

    Ahorn.drawImage(ctx, midSprite, x + div(width - midSprite.width, 2), y + div(height - midSprite.height, 2))
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableSwapBlock, room::Maple.Room)
    sprite = get(entity.data, "sprite", "block")
    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    stopX, stopY = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    frame, trail, mid = getTextures(entity)

    renderSwapBlock(ctx, stopX, stopY, width, height, mid, frame)
    Ahorn.drawArrow(ctx, startX + width / 2, startY + height / 2, stopX + width / 2, stopY + height / 2, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableSwapBlock, room::Maple.Room)
    sprite = get(entity.data, "sprite", "block")

    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    stopX, stopY = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    frame, trail, mid = getTextures(entity)

    renderTrail(ctx, min(startX, stopX), min(startY, stopY), abs(startX - stopX) + width, abs(startY - stopY) + height, trail)
    renderSwapBlock(ctx, startX, startY, width, height, mid, frame)
end

end