module MaxHelpingHandMovingFlagTouchSwitch

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/MovingFlagTouchSwitch" MovingFlagTouchSwitch(x::Integer, y::Integer,
    flag::String="moving_flag_touch_switch", icon::String="vanilla", persistent::Bool=false,
    inactiveColor::String="5FCDE4", movingColor::String="FF8080", activeColor::String="FFFFFF", finishColor::String="F141DF")

const placements = Ahorn.PlacementDict(
    "Flag Touch Switch (Moving)\n(max480's Helping Hand + Outback Helper)" => Ahorn.EntityPlacement(
        MovingFlagTouchSwitch
    )
)

const bundledIcons = String["vanilla", "tall", "triangle", "circle"]

Ahorn.editingOptions(entity::MovingFlagTouchSwitch) = Dict{String,Any}(
    "icon" => bundledIcons
)

Ahorn.editingOrder(entity::MovingFlagTouchSwitch) = String["x", "y", "width", "height", "inactiveColor", "movingColor", "activeColor", "finishColor"]

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::MovingFlagTouchSwitch)
    px, py = Ahorn.position(entity)

    sprite = "collectables/outback/movingtouchswitch/container.png"

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        theta = atan(py - ny, px - nx)
        Ahorn.drawArrow(ctx, px, py, nx + cos(theta) * 8, ny + sin(theta) * 8, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx, ny)

        px, py = nx, ny
    end
end

function Ahorn.selection(entity::MovingFlagTouchSwitch)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    sprite = "collectables/outback/movingtouchswitch/container.png"

    res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(sprite, x, y)]
    
    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.getSpriteRectangle(sprite, nx, ny))
    end

    return res
end

Ahorn.nodeLimits(entity::MovingFlagTouchSwitch) = 0, -1

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MovingFlagTouchSwitch)
    icon = get(entity.data, "icon", "vanilla")
    
    iconPath = "objects/touchswitch/icon00.png"
    if icon != "vanilla"
        iconPath = "objects/MaxHelpingHand/flagTouchSwitch/$(icon)/icon00.png"
    end

    Ahorn.drawSprite(ctx, "collectables/outback/movingtouchswitch/container.png", 0, 0)
    Ahorn.drawSprite(ctx, iconPath, 0, 0)
end

end
