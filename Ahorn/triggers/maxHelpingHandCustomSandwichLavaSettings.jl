module MaxHelpingHandCustomSandwichLavaSettingsTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/CustomSandwichLavaSettingsTrigger" CustomSandwichLavaSettingsTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    onlyOnce::Bool=false, direction::String="CoreModeBased", speed::Number=20.0)

const directions = String["AlwaysUp", "AlwaysDown", "CoreModeBased"]

const placements = Ahorn.PlacementDict(
    "Custom Sandwich Lava Settings (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CustomSandwichLavaSettingsTrigger,
        "rectangle",
    ),
)

Ahorn.editingOptions(entity::CustomSandwichLavaSettingsTrigger) = Dict{String, Any}(
    "direction" => directions
)

end
