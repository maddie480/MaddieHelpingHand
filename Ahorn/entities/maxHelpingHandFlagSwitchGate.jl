module MaxHelpingHandFlagSwitchGate

using ..Ahorn, Maple

@pardef FlagSwitchGate(x1::Integer, y1::Integer, x2::Integer=x1+16, y2::Integer=y1, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    sprite::String="block", persistent::Bool=false, flag::String="flag_touch_switch", icon::String="vanilla", inactiveColor::String="5FCDE4", activeColor::String="FFFFFF", finishColor::String="F141DF",
    shakeTime::Number=0.5, moveTime::Number=1.8, moveEased::Bool=true, allowReturn::Bool=false, moveSound::String="event:/game/general/touchswitch_gate_open", finishedSound::String="event:/game/general/touchswitch_gate_finish",
    smoke::Bool=true, surfaceIndex::Int16=convert(Int16, 8)) =
    Entity("MaxHelpingHand/FlagSwitchGate", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], width=width, height=height, sprite=sprite, persistent=persistent, flag=flag, icon=icon,
    inactiveColor=inactiveColor, activeColor=activeColor, finishColor=finishColor, shakeTime=shakeTime, moveTime=moveTime, moveEased=moveEased, allowReturn=allowReturn, moveSound=moveSound, finishedSound=finishedSound,
    smoke=smoke, surfaceIndex=surfaceIndex)

function gateFinalizer(entity)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    entity.data["nodes"] = [(x + width, y)]
end

const textures = String["block", "mirror", "temple", "stars"]
const bundledIcons = String["vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged", "cross", "drop", "hourglass", "split", "star", "triple"]

const placements = Ahorn.PlacementDict(
    "Flag Switch Gate ($(uppercasefirst(texture))) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        FlagSwitchGate,
        "rectangle",
        Dict{String, Any}(
            "sprite" => texture
        ),
        gateFinalizer
    ) for texture in textures
)

Ahorn.editingOrder(entity::FlagSwitchGate) = String["x", "y", "width", "height", "flag", "inactiveColor", "activeColor", "finishColor", "hitSound", "moveSound", "finishedSound", "shakeTime", "moveTime"]

Ahorn.editingOptions(entity::FlagSwitchGate) = Dict{String, Any}(
    "sprite" => textures,
    "icon" => bundledIcons,
    "surfaceIndex" => Maple.tileset_sound_ids
)

Ahorn.nodeLimits(entity::FlagSwitchGate) = 1, 1

Ahorn.minimumSize(entity::FlagSwitchGate) = 16, 16
Ahorn.resizable(entity::FlagSwitchGate) = true, true

function Ahorn.selection(entity::FlagSwitchGate)
    x, y = Ahorn.position(entity)
    stopX, stopY = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(stopX, stopY, width, height)]
end

function renderGateSwitch(ctx::Ahorn.Cairo.CairoContext, entity::FlagSwitchGate, x::Number, y::Number, width::Number, height::Number, sprite::String)
    icon = get(entity.data, "icon", "vanilla")

    iconResource = "objects/switchgate/icon00"
    if icon != "vanilla"
        iconResource = "objects/MaxHelpingHand/flagSwitchGate/$(icon)/icon00"
    end

    iconSprite = Ahorn.getSprite(iconResource, "Gameplay")

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

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::FlagSwitchGate, room::Maple.Room)
    sprite = get(entity.data, "sprite", "block")
    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    stopX, stopY = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    renderGateSwitch(ctx, entity, stopX, stopY, width, height, sprite)
    Ahorn.drawArrow(ctx, startX + width / 2, startY + height / 2, stopX + width / 2, stopY + height / 2, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FlagSwitchGate, room::Maple.Room)
    sprite = get(entity.data, "sprite", "block")

    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    renderGateSwitch(ctx, entity, x, y, width, height, sprite)
end

end