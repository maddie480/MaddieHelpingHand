module MaxHelpingHandFlagTouchSwitch

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/FlagTouchSwitch" FlagTouchSwitch(x::Integer, y::Integer, 
    flag::String="flag_touch_switch", icon::String="vanilla", persistent::Bool=false,
    inactiveColor::String="5FCDE4", activeColor::String="FFFFFF", finishColor::String="F141DF", smoke::Bool=true,
    inverted::Bool=false, allowDisable::Bool=false, hitSound::String="event:/game/general/touchswitch_any",
    completeSoundFromSwitch::String="event:/game/general/touchswitch_last_cutoff", completeSoundFromScene::String="event:/game/general/touchswitch_last_oneshot")
    
const bundledIcons = String["vanilla", "tall", "triangle", "circle", "diamond", "double", "heart", "square", "wide", "winged"]

const placements = Ahorn.PlacementDict(
    "Flag Touch Switch (max480's Helping Hand)" => Ahorn.EntityPlacement(
        FlagTouchSwitch
    )
)

Ahorn.editingOrder(entity::FlagTouchSwitch) = String["x", "y", "inactiveColor", "activeColor", "finishColor", "hitSound", "completeSoundFromSwitch", "completeSoundFromScene"]

Ahorn.editingOptions(entity::FlagTouchSwitch) = Dict{String,Any}(
    "icon" => bundledIcons
)

function Ahorn.selection(entity::FlagTouchSwitch)
    x, y = Ahorn.position(entity)

    return  Ahorn.Rectangle(x - 7, y - 7, 14, 14)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlagTouchSwitch, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/touchswitch/container.png", 0, 0)

    icon = get(entity.data, "icon", "vanilla")
    
    iconPath = "objects/touchswitch/icon00.png"
    if icon != "vanilla"
        iconPath = "objects/MaxHelpingHand/flagTouchSwitch/$(icon)/icon00.png"
    end

    Ahorn.drawSprite(ctx, iconPath, 0, 0)
end

end