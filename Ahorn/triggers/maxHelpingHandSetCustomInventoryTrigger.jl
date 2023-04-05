module MaxHelpingHandSetCustomInventoryTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/SetCustomInventoryTrigger" SetCustomInventoryTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    dashes::Integer=1, backpack::Bool=true, dreamDash::Bool=false, groundRefills::Bool=true)

const placements = Ahorn.PlacementDict(
    "Set Custom Inventory Trigger (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        SetCustomInventoryTrigger,
        "rectangle"
    )
)

end