module MaxHelpingHandReskinnableSwitchGate

using ..Ahorn, Maple

@pardef ReskinnableSwitchGate(x1::Integer, y1::Integer, x2::Integer=x1+16, y2::Integer=y1, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    persistent::Bool=false, sprite::String="block", icon::String="objects/switchgate/icon", inactiveColor::String="5fcde4", activeColor::String="ffffff",
    finishColor::String="f141df", surfaceIndex::Int16=convert(Int16, 8)) =
    Entity("MaxHelpingHand/ReskinnableSwitchGate", x=x1, y=y1, nodes=Tuple{Int, Int}[(x2, y2)], width=width, height=height, persistent=persistent, sprite=sprite, icon=icon,
    inactiveColor=inactiveColor, activeColor=activeColor, finishColor=finishColor, surfaceIndex=surfaceIndex)


function gateFinalizer(entity)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    entity.data["nodes"] = [(x + width, y)]
end

const textures = ["block", "mirror", "temple", "stars"]

const placements = Ahorn.PlacementDict(
    "Switch Gate (Stone, Reskinnable) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableSwitchGate,
        "rectangle",
        Dict{String, Any}(
            "sprite" => "block"
        ),
        gateFinalizer
    ),
    "Switch Gate (Mirror, Reskinnable) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableSwitchGate,
        "rectangle",
        Dict{String, Any}(
            "sprite" => "mirror"
        ),
        gateFinalizer
    ),
    "Switch Gate (Temple, Reskinnable) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableSwitchGate,
        "rectangle",
        Dict{String, Any}(
            "sprite" => "temple"
        ),
        gateFinalizer
    ),
    "Switch Gate (Moon, Reskinnable) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableSwitchGate,
        "rectangle",
        Dict{String, Any}(
            "sprite" => "stars"
        ),
        gateFinalizer
    )
)

Ahorn.editingOptions(entity::ReskinnableSwitchGate) = Dict{String, Any}(
    "sprite" => textures,
    "icon" => [
        "objects/switchgate/icon",
        "objects/MaxHelpingHand/flagSwitchGate/tall/icon",
        "objects/MaxHelpingHand/flagSwitchGate/triangle/icon",
        "objects/MaxHelpingHand/flagSwitchGate/circle/icon",
        "objects/MaxHelpingHand/flagSwitchGate/diamond/icon",
        "objects/MaxHelpingHand/flagSwitchGate/double/icon",
        "objects/MaxHelpingHand/flagSwitchGate/heart/icon",
        "objects/MaxHelpingHand/flagSwitchGate/square/icon",
        "objects/MaxHelpingHand/flagSwitchGate/wide/icon",
        "objects/MaxHelpingHand/flagSwitchGate/winged/icon"
    ],
    "surfaceIndex" => Maple.tileset_sound_ids
)

Ahorn.nodeLimits(entity::ReskinnableSwitchGate) = 1, 1

Ahorn.minimumSize(entity::ReskinnableSwitchGate) = 16, 16
Ahorn.resizable(entity::ReskinnableSwitchGate) = true, true

function Ahorn.selection(entity::ReskinnableSwitchGate)
    x, y = Ahorn.position(entity)
    stopX, stopY = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return [Ahorn.Rectangle(x, y, width, height), Ahorn.Rectangle(stopX, stopY, width, height)]
end

function renderGateSwitch(ctx::Ahorn.Cairo.CairoContext, x::Int, y::Int, width::Int, height::Int, sprite::String, iconPath::String)
    iconSprite = Ahorn.getSprite(iconPath * "00", "Gameplay")

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

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableSwitchGate, room::Maple.Room)
    sprite = get(entity.data, "sprite", "block")
    startX, startY = Int(entity.data["x"]), Int(entity.data["y"])
    stopX, stopY = Int.(entity.data["nodes"][1])

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    renderGateSwitch(ctx, stopX, stopY, width, height, sprite, entity.icon)
    Ahorn.drawArrow(ctx, startX + width / 2, startY + height / 2, stopX + width / 2, stopY + height / 2, Ahorn.colors.selection_selected_fc, headLength=6)
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableSwitchGate, room::Maple.Room)
    sprite = get(entity.data, "sprite", "block")

    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    renderGateSwitch(ctx, x, y, width, height, sprite, entity.icon)
end

end
