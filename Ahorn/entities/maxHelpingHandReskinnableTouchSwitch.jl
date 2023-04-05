module MaxHelpingHandReskinnableTouchSwitch

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/ReskinnableTouchSwitch" ReskinnableTouchSwitch(x::Integer, y::Integer, icon::String="objects/touchswitch/icon",
    borderTexture::String="objects/touchswitch/container", inactiveColor::String="5fcde4", activeColor::String="ffffff", finishColor::String="f141df")

const placements = Ahorn.PlacementDict(
    "Touch Switch (Reskinnable) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableTouchSwitch
    )
)

Ahorn.editingOptions(entity::ReskinnableTouchSwitch) = Dict{String, Any}(
    "icon" => [
        "objects/touchswitch/icon",
        "objects/MaxHelpingHand/flagTouchSwitch/tall/icon",
        "objects/MaxHelpingHand/flagTouchSwitch/triangle/icon",
        "objects/MaxHelpingHand/flagTouchSwitch/circle/icon",
        "objects/MaxHelpingHand/flagTouchSwitch/diamond/icon",
        "objects/MaxHelpingHand/flagTouchSwitch/double/icon",
        "objects/MaxHelpingHand/flagTouchSwitch/heart/icon",
        "objects/MaxHelpingHand/flagTouchSwitch/square/icon",
        "objects/MaxHelpingHand/flagTouchSwitch/wide/icon",
        "objects/MaxHelpingHand/flagTouchSwitch/winged/icon"
    ]
)

function Ahorn.selection(entity::ReskinnableTouchSwitch)
    x, y = Ahorn.position(entity)

    return  Ahorn.Rectangle(x - 7, y - 7, 14, 14)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableTouchSwitch, room::Maple.Room)
    Ahorn.drawSprite(ctx, entity.borderTexture, 0, 0)
    Ahorn.drawSprite(ctx, entity.icon * "00", 0, 0)
end

end
