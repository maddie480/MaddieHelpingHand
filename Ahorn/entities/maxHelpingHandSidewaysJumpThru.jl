module MaxHelpingHandSidewaysJumpThru

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/SidewaysJumpThru" SidewaysJumpThru(x::Integer, y::Integer, height::Integer=Maple.defaultBlockHeight, 
    left::Bool=true, texture::String="wood", animationDelay::Number=0.0, letSeekersThrough::Bool=false, surfaceIndex::Int=-1, pushPlayer::Bool=false)
@mapdef Entity "MaxHelpingHand/AttachedSidewaysJumpThru" AttachedSidewaysJumpThru(x::Integer, y::Integer, height::Integer=Maple.defaultBlockHeight, 
    left::Bool=true, texture::String="wood", animationDelay::Number=0.0, letSeekersThrough::Bool=false, surfaceIndex::Int=-1, pushPlayer::Bool=false)

textures = ["wood", "dream", "temple", "templeB", "cliffside", "reflection", "core", "moon"]
const placements = Ahorn.PlacementDict(
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

quads = Tuple{Integer, Integer, Integer, Integer}[
    (0, 0, 8, 7) (8, 0, 8, 7) (16, 0, 8, 7);
    (0, 8, 8, 5) (8, 8, 8, 5) (16, 8, 8, 5)
]

Ahorn.editingOptions(entity::jumpthruUnion) = Dict{String, Any}(
    "texture" => textures,
    "surfaceIndex" => Maple.tileset_sound_ids
)

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