module MaxHelpingHandShatterFlagSwitchGate

using ..Ahorn, Maple

@pardef ShatterFlagSwitchGate(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    sprite::String="block", persistent::Bool=false, flag::String="flag_touch_switch", icon::String="vanilla", inactiveColor::String="5FCDE4", activeColor::String="FFFFFF", finishColor::String="F141DF",
    shakeTime::Number=0.5, finishedSound::String="event:/game/general/touchswitch_gate_finish", debrisPath::String="") =
    Entity("MaxHelpingHand/ShatterFlagSwitchGate", x=x, y=y, width=width, height=height,
    sprite=sprite, persistent=persistent, flag=flag, icon=icon, inactiveColor=inactiveColor, activeColor=activeColor, finishColor=finishColor,
    shakeTime=shakeTime, finishedSound=finishedSound, debrisPath=debrisPath)

const textures = String["block", "mirror", "temple", "stars"]
const bundledIcons = String["vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged"]

const placements = Ahorn.PlacementDict(
    "Shatter Flag Switch Gate ($(uppercasefirst(texture)))\n(max480's Helping Hand + Vortex Helper)" => Ahorn.EntityPlacement(
        ShatterFlagSwitchGate,
        "rectangle",
        Dict{String, Any}(
            "sprite" => texture
        )
    ) for texture in textures
)

Ahorn.editingOrder(entity::ShatterFlagSwitchGate) = String["x", "y", "width", "height", "flag", "inactiveColor", "activeColor", "finishColor", "finishedSound", "shakeTime"]

Ahorn.editingOptions(entity::ShatterFlagSwitchGate) = Dict{String, Any}(
    "sprite" => textures,
    "icon" => bundledIcons
)

Ahorn.minimumSize(entity::ShatterFlagSwitchGate) = 16, 16
Ahorn.resizable(entity::ShatterFlagSwitchGate) = true, true

function Ahorn.selection(entity::ShatterFlagSwitchGate)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height)]
end

function renderGateSwitch(ctx::Ahorn.Cairo.CairoContext, entity::ShatterFlagSwitchGate, x::Number, y::Number, width::Number, height::Number, sprite::String)
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

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ShatterFlagSwitchGate, room::Maple.Room)
    sprite = get(entity.data, "sprite", "block")

    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    renderGateSwitch(ctx, entity, x, y, width, height, sprite)
end

end