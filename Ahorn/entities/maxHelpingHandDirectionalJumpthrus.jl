module MaxHelpingHandDirectionalJumpThrus

using ..Ahorn, Maple

# ==== Upside-down jumpthrus

@mapdef Entity "MaxHelpingHand/UpsideDownJumpThru" UpsideDownJumpThru(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, texture::String="wood",
    animationDelay::Number=0.0, pushPlayer::Bool=false, attached::Bool=false)

const textures = ["wood", "dream", "temple", "templeB", "cliffside", "reflection", "core", "moon"]

const quads = Tuple{Integer, Integer, Integer, Integer}[
    (0, 0, 8, 7) (8, 0, 8, 7) (16, 0, 8, 7);
    (0, 8, 8, 5) (8, 8, 8, 5) (16, 8, 8, 5)
]

Ahorn.editingOptions(entity::UpsideDownJumpThru) = Dict{String, Any}(
    "texture" => textures
)

Ahorn.minimumSize(entity::UpsideDownJumpThru) = 8, 0
Ahorn.resizable(entity::UpsideDownJumpThru) = true, false

function Ahorn.selection(entity::UpsideDownJumpThru)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))

    return Ahorn.Rectangle(x, y, width, 8)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::UpsideDownJumpThru, room::Maple.Room)
    texture = get(entity.data, "texture", "wood")
    texture = texture == "default" ? "wood" : texture

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 8))

    startX = div(x, 8) + 1
    stopX = startX + div(width, 8) - 1
    startY = div(y, 8) + 1
    animated = Number(get(entity.data, "animationDelay", 0)) > 0

    Ahorn.Cairo.save(ctx)

    Ahorn.scale(ctx, 1, -1)

    len = stopX - startX
    for i in 0:len
        if animated
            Ahorn.drawImage(ctx, "objects/jumpthru/$(texture)00", 8 * i, -8)
        else
            connected = false
            qx = 2
            if i == 0
                connected = get(room.fgTiles.data, (startY, startX - 1), false) != '0'
                qx = 1

            elseif i == len
                connected = get(room.fgTiles.data, (startY, stopX + 1), false) != '0'
                qx = 3
            end

            quad = quads[2 - connected, qx]
            Ahorn.drawImage(ctx, "objects/jumpthru/$(texture)", 8 * i, -8, quad...)
        end
    end

    Ahorn.Cairo.restore(ctx)
end

function Ahorn.flipped(entity::UpsideDownJumpThru, horizontal::Bool)
    if !horizontal
        return RegularJumpThru(entity.x, entity.y, entity.width, entity.texture)
    end
end

Ahorn.rotated(entity::UpsideDownJumpThru, steps::Int) = SidewaysJumpThru(entity.x, entity.y, entity.width, steps > 0, entity.texture)


# ==== regular jumpthru got from the result of rotating or flipping helping hand jumpthrus

@mapdef Entity "MaxHelpingHand/RegularJumpThru" RegularJumpThru(x::Integer, y::Integer, width::Integer=8, texture::String="wood", surfaceIndex::Int16=convert(Int16, -1))

Ahorn.editingOptions(entity::RegularJumpThru) = Dict{String, Any}(
    "texture" => textures,
    "surfaceIndex" => Maple.tileset_sound_ids
)

Ahorn.minimumSize(entity::RegularJumpThru) = 8, 0
Ahorn.resizable(entity::RegularJumpThru) = true, false

function Ahorn.selection(entity::RegularJumpThru)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))

    return Ahorn.Rectangle(x, y, width, 8)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RegularJumpThru, room::Maple.Room)
    texture = get(entity.data, "texture", "wood")
    texture = texture == "default" ? "wood" : texture

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 8))

    startX = div(x, 8) + 1
    stopX = startX + div(width, 8) - 1
    startY = div(y, 8) + 1

    len = stopX - startX
    for i in 0:len
        connected = false
        qx = 2
        if i == 0
            connected = get(room.fgTiles.data, (startY, startX - 1), false) != '0'
            qx = 1

        elseif i == len
            connected = get(room.fgTiles.data, (startY, stopX + 1), false) != '0'
            qx = 3
        end

        quad = quads[2 - connected, qx]
        Ahorn.drawImage(ctx, "objects/jumpthru/$(texture)", 8 * i, 0, quad[1], quad[2], quad[3], quad[4])
    end
end

function Ahorn.flipped(entity::RegularJumpThru, horizontal::Bool)
    if !horizontal
        return UpsideDownJumpThru(entity.x, entity.y, entity.width, entity.texture)
    end
end

Ahorn.rotated(entity::RegularJumpThru, steps::Int) = SidewaysJumpThru(entity.x, entity.y, entity.width, steps < 0, entity.texture)

# ==== Sideways jumpthrus

@mapdef Entity "MaxHelpingHand/SidewaysJumpThru" SidewaysJumpThru(x::Integer, y::Integer, height::Integer=Maple.defaultBlockHeight,
    left::Bool=true, texture::String="wood", animationDelay::Number=0.0, letSeekersThrough::Bool=false, surfaceIndex::Int=-1, pushPlayer::Bool=false)
@mapdef Entity "MaxHelpingHand/AttachedSidewaysJumpThru" AttachedSidewaysJumpThru(x::Integer, y::Integer, height::Integer=Maple.defaultBlockHeight,
    left::Bool=true, texture::String="wood", animationDelay::Number=0.0, letSeekersThrough::Bool=false, surfaceIndex::Int=-1, pushPlayer::Bool=false)

const placements = Ahorn.PlacementDict(
    "Upside Down Jump Through (max480's Helping Hand)" => Ahorn.EntityPlacement(
        UpsideDownJumpThru,
        "rectangle",
        Dict{String, Any}(
            "texture" => "wood"
        )
    ),
    "Sideways Jump Through (Left) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        SidewaysJumpThru,
        "rectangle",
        Dict{String, Any}(
            "texture" => "wood",
            "left" => true
        )
    ),
    "Sideways Jump Through (Right) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        SidewaysJumpThru,
        "rectangle",
        Dict{String, Any}(
            "texture" => "wood",
            "left" => false
        )
    ),
    "Sideways Jump Through (Attached, Left)\n(max480's Helping Hand)" => Ahorn.EntityPlacement(
        AttachedSidewaysJumpThru,
        "rectangle",
        Dict{String, Any}(
            "texture" => "wood",
            "left" => true
        )
    ),
    "Sideways Jump Through (Attached, Right)\n(max480's Helping Hand)" => Ahorn.EntityPlacement(
        AttachedSidewaysJumpThru,
        "rectangle",
        Dict{String, Any}(
            "texture" => "wood",
            "left" => false
        )
    )
)

# Sideways Jumpthrus and Attached ones look the same and have the same options, so we're going to handle them together!
const jumpthruUnion = Union{SidewaysJumpThru, AttachedSidewaysJumpThru}

Ahorn.editingOptions(entity::jumpthruUnion) = Dict{String, Any}(
    "texture" => textures,
    "surfaceIndex" => Maple.tileset_sound_ids
)

function Ahorn.flipped(entity::jumpthruUnion, horizontal::Bool)
    if horizontal
        entity.left = !entity.left
        return entity
    end
end

function Ahorn.rotated(entity::jumpthruUnion, steps::Int)
    if (steps > 0) == entity.left
        return RegularJumpThru(entity.x, entity.y, entity.height, entity.texture, convert(Int16, entity.surfaceIndex))
    else
        return UpsideDownJumpThru(entity.x, entity.y, entity.height, entity.texture)
    end
end

Ahorn.minimumSize(entity::jumpthruUnion) = 0, 8
Ahorn.resizable(entity::jumpthruUnion) = false, true

function Ahorn.selection(entity::jumpthruUnion)
    x, y = Ahorn.position(entity)
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, 8, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::jumpthruUnion, room::Maple.Room)
    texture = get(entity.data, "texture", "wood")
    texture = texture == "default" ? "wood" : texture

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    height = Int(get(entity.data, "height", 8))
    left = get(entity.data, "left", true)

    startX = div(x, 8) + 1
    startY = div(y, 8) + 1
    stopY = startY + div(height, 8) - 1
    animated = Number(get(entity.data, "animationDelay", 0)) > 0

    Ahorn.Cairo.save(ctx)

    Ahorn.rotate(ctx, pi / 2)

    if left
        Ahorn.scale(ctx, 1, -1)
    end

    len = stopY - startY
    for i in 0:len
        if animated
            Ahorn.drawImage(ctx, "objects/jumpthru/$(texture)00", 8 * i, left ? 0 : -8)
        else
            connected = false
            qx = 2
            if i == 0
                connected = get(room.fgTiles.data, (startY - 1, startX), false) != '0'
                qx = 1

            elseif i == len
                connected = get(room.fgTiles.data, (stopY + 1, startX), false) != '0'
                qx = 3
            end

            quad = quads[2 - connected, qx]
            Ahorn.drawImage(ctx, "objects/jumpthru/$(texture)", 8 * i, left ? 0 : -8, quad...)
        end
    end

    Ahorn.Cairo.restore(ctx)
end

end