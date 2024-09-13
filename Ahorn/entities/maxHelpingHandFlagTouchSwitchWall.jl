module MaxHelpingHandFlagTouchSwitchWall

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/FlagTouchSwitchWall" FlagTouchSwitchWall(x::Integer, y::Integer, width::Integer=16, height::Integer=16,
    flag::String="flag_touch_switch", icon::String="vanilla", persistent::Bool=false, hideIfFlag::String="",
    inactiveColor::String="5FCDE4", activeColor::String="FFFFFF", finishColor::String="F141DF", smoke::Bool=true, animationLength::Integer=6,
    inverted::Bool=false, allowDisable::Bool=false, playerCanActivate::Bool=true, hitSound::String="event:/game/general/touchswitch_any",
    completeSoundFromSwitch::String="event:/game/general/touchswitch_last_cutoff", completeSoundFromScene::String="event:/game/general/touchswitch_last_oneshot")

const bundledIcons = String["vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged", "cross", "drop", "hourglass", "split", "star", "triple"]

const placements = Ahorn.PlacementDict(
    "Flag Touch Switch Wall (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        FlagTouchSwitchWall
    )
)

Ahorn.editingOrder(entity::FlagTouchSwitchWall) = String["x", "y", "width", "height", "inactiveColor", "activeColor", "finishColor", "hitSound", "completeSoundFromSwitch", "completeSoundFromScene"]

Ahorn.editingOptions(entity::FlagTouchSwitchWall) = Dict{String,Any}(
    "icon" => bundledIcons
)

Ahorn.minimumSize(entity::FlagTouchSwitchWall) = 8, 8
Ahorn.resizable(entity::FlagTouchSwitchWall) = true, true
Ahorn.selection(entity::FlagTouchSwitchWall) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlagTouchSwitchWall, room::Maple.Room)
    icon = get(entity.data, "icon", "vanilla")

    iconPath = "objects/touchswitch/icon00.png"
    if icon != "vanilla"
        iconPath = "objects/MaxHelpingHand/flagTouchSwitch/$(icon)/icon00.png"
    end

    width = get(entity.data, "width", 8)
    height = get(entity.data, "height", 8)

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.0, 0.0, 0.0, 0.3), (1.0, 1.0, 1.0, 0.5))
    Ahorn.drawSprite(ctx, iconPath, width / 2, height / 2)
end

end