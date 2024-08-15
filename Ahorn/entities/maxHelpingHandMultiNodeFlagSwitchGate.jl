module MaxHelpingHandMultiNodeFlagSwitchGate

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/MultiNodeFlagSwitchGate" MultiNodeFlagSwitchGate(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
    inactiveColor::String="5FCDE4", activeColor::String="FFFFFF", finishColor::String="F141DF", flags::String="switch1,switch2", shakeTime::Number=0.5, moveTime::Number=2.0, easing::String="CubeOut",
    sprite::String="block", icon::String="vanilla", resetFlags::Bool=true, canReturn::Bool=true, progressionMode::Bool=false, persistent::Bool=true, pauseTimes::String="",
    pauseTimeBeforeFirstMove::Number=0.0, doNotSkipNodes::Bool=false, smoke::Bool=true, moveSound::String="event:/game/general/touchswitch_gate_open", finishedSound::String="event:/game/general/touchswitch_gate_finish")

function gateFinalizer(entity)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    entity.data["nodes"] = [(x + width, y)]
end

const textures = ["block", "mirror", "temple", "stars"]
const iconTypes = String["vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged", "cross", "drop", "hourglass", "split", "star", "triple"]

const easeTypes = String["Linear", "SineIn", "SineOut", "SineInOut", "QuadIn", "QuadOut", "QuadInOut", "CubeIn", "CubeOut", "CubeInOut", "QuintIn", "QuintOut", "QuintInOut", "BackIn", "BackOut", "BackInOut", "ExpoIn", "ExpoOut", "ExpoInOut", "BigBackIn", "BigBackOut", "BigBackInOut", "ElasticIn", "ElasticOut", "ElasticInOut", "BounceIn", "BounceOut", "BounceInOut"]

const placements = Ahorn.PlacementDict(
    "Flag Switch Gate ($(uppercasefirst(texture)), Multi-Node) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        MultiNodeFlagSwitchGate,
        "rectangle",
        Dict{String, Any}(
            "sprite" => texture
        ),
        gateFinalizer
    ) for texture in textures
)

Ahorn.editingOptions(entity::MultiNodeFlagSwitchGate) = Dict{String, Any}(
    "sprite" => textures,
    "icon" => iconTypes,
    "easing" => easeTypes
)

Ahorn.nodeLimits(entity::MultiNodeFlagSwitchGate) = 1, -1

Ahorn.minimumSize(entity::MultiNodeFlagSwitchGate) = 16, 16
Ahorn.resizable(entity::MultiNodeFlagSwitchGate) = true, true

function Ahorn.selection(entity::MultiNodeFlagSwitchGate)
    x, y = Ahorn.position(entity)
    stopX, stopY = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    res = [Ahorn.Rectangle(x, y, width, height)]

    for node in get(entity.data, "nodes", ())
        stopX, stopY = Int.(node)
        push!(res, Ahorn.Rectangle(stopX, stopY, width, height))
    end

    return res
end

iconResource = "objects/switchgate/icon00"

function renderGateSwitch(ctx::Ahorn.Cairo.CairoContext, x::Number, y::Number, width::Number, height::Number, sprite::String, icon::String)
    iconSprite = Ahorn.getSprite(icon, "Gameplay")

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    frame = "objects/switchgate/$sprite"

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

    Ahorn.drawImage(ctx, iconSprite, x + div(width - iconSprite.width, 2), y + div(height - iconSprite.height, 2))
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::MultiNodeFlagSwitchGate, room::Maple.Room)
    sprite = get(entity.data, "sprite", "block")
    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    iconName = get(entity.data, "icon", "vanilla")
    icon = "objects/switchgate/icon00"
    if iconName != "vanilla"
        icon = "objects/MaxHelpingHand/flagSwitchGate/"*iconName*"/icon00"
    end

    for node in get(entity.data, "nodes", ())
        stopX, stopY = Int.(node)

        renderGateSwitch(ctx, stopX, stopY, width, height, sprite, icon)
        Ahorn.drawArrow(ctx, startX + width / 2, startY + height / 2, stopX + width / 2, stopY + height / 2, Ahorn.colors.selection_selected_fc, headLength=6)
        startX, startY = stopX, stopY
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::MultiNodeFlagSwitchGate, room::Maple.Room)
    sprite = get(entity.data, "sprite", "block")

    iconName = get(entity.data, "icon", "vanilla")
    icon = "objects/switchgate/icon00"
    if iconName != "vanilla"
        icon = "objects/MaxHelpingHand/flagSwitchGate/"*iconName*"/icon00"
    end

    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    renderGateSwitch(ctx, x, y, width, height, sprite, icon)
end

end