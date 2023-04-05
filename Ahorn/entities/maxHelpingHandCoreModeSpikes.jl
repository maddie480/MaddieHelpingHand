module MaxHelpingHandCoreModeSpikes

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CoreModeSpikesUp" CoreModeSpikesUp(x::Integer, y::Integer, width::Integer=Maple.defaultSpikeWidth, hotType::String="MaxHelpingHand/heatspike", coldType::String="cliffside")
@mapdef Entity "MaxHelpingHand/CoreModeSpikesDown" CoreModeSpikesDown(x::Integer, y::Integer, width::Integer=Maple.defaultSpikeWidth, hotType::String="MaxHelpingHand/heatspike", coldType::String="cliffside")
@mapdef Entity "MaxHelpingHand/CoreModeSpikesLeft" CoreModeSpikesLeft(x::Integer, y::Integer, height::Integer=Maple.defaultSpikeHeight, hotType::String="MaxHelpingHand/heatspike", coldType::String="cliffside")
@mapdef Entity "MaxHelpingHand/CoreModeSpikesRight" CoreModeSpikesRight(x::Integer, y::Integer, height::Integer=Maple.defaultSpikeHeight, hotType::String="MaxHelpingHand/heatspike", coldType::String="cliffside")

const spikeTypes = String[
    "default",
    "outline",
    "cliffside",
    "reflection",
    "MaxHelpingHand/heatspike"
]

const entities = Dict{String, Type}(
    "up" => CoreModeSpikesUp,
    "down" => CoreModeSpikesDown,
    "left" => CoreModeSpikesLeft,
    "right" => CoreModeSpikesRight
)

const spikesUnion = Union{CoreModeSpikesUp, CoreModeSpikesDown, CoreModeSpikesLeft, CoreModeSpikesRight}

const placements = Ahorn.PlacementDict()
for (dir, entity) in entities
    key = "Core Mode Spikes ($(uppercasefirst(dir))) (Maddie's Helping Hand)"
    placements[key] = Ahorn.EntityPlacement(
        entity,
        "rectangle"
    )
end

Ahorn.editingOptions(entity::spikesUnion) = Dict{String, Any}(
    "hotType" => spikeTypes,
    "coldType" => spikeTypes
)

const directions = Dict{String, String}(
    "MaxHelpingHand/CoreModeSpikesUp" => "up",
    "MaxHelpingHand/CoreModeSpikesDown" => "down",
    "MaxHelpingHand/CoreModeSpikesLeft" => "left",
    "MaxHelpingHand/CoreModeSpikesRight" => "right"
)

const offsets = Dict{String, Tuple{Integer, Integer}}(
    "up" => (4, -4),
    "down" => (4, 4),
    "left" => (-4, 4),
    "right" => (4, 4)
)

const rotations = Dict{String, Number}(
    "up" => 0,
    "right" => pi / 2,
    "down" => pi,
    "left" => pi * 3 / 2
)

const resizeDirections = Dict{String, Tuple{Bool, Bool}}(
    "up" => (true, false),
    "down" => (true, false),
    "left" => (false, true),
    "right" => (false, true),
)

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::spikesUnion)
    direction = get(directions, entity.name, "up")
    theta = rotations[direction] - pi / 2

    width = Int(get(entity.data, "width", 0))
    height = Int(get(entity.data, "height", 0))

    x, y = Ahorn.position(entity)
    cx, cy = x + floor(Int, width / 2) - 8 * (direction == "left"), y + floor(Int, height / 2) - 8 * (direction == "up")

    Ahorn.drawArrow(ctx, cx, cy, cx + cos(theta) * 24, cy + sin(theta) * 24, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.selection(entity::spikesUnion)
    if haskey(directions, entity.name)
        x, y = Ahorn.position(entity)

        width = Int(get(entity.data, "width", 8))
        height = Int(get(entity.data, "height", 8))

        direction = get(directions, entity.name, "up")
        variant = get(entity.data, "hotType", "default")

        width = Int(get(entity.data, "width", 8))
        height = Int(get(entity.data, "height", 8))

        ox, oy = offsets[direction]

        return Ahorn.Rectangle(x + ox - 4, y + oy - 4, width, height)
    end
end

Ahorn.minimumSize(entity::spikesUnion) = (8, 8)

function Ahorn.resizable(entity::spikesUnion)
    if haskey(directions, entity.name)
        variant = get(entity.data, "hotType", "default")
        direction = get(directions, entity.name, "up")

        return resizeDirections[direction]
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::spikesUnion)
    if haskey(directions, entity.name)
        variant = get(entity.data, "hotType", "default")
        direction = get(directions, entity.name, "up")

        width = get(entity.data, "width", 8)
        height = get(entity.data, "height", 8)

        for ox in 0:8:width - 8, oy in 0:8:height - 8
            drawX = ox + offsets[direction][1]
            drawY = oy + offsets[direction][2]

            Ahorn.drawSprite(ctx, "danger/spikes/$(variant)_$(direction)00", drawX, drawY)
        end
    end
end

end
