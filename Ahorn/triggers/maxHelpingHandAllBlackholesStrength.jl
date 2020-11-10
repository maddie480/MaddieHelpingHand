module MaxHelpingHandAllBlackholesStrengthTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/AllBlackholesStrengthTrigger" AllBlackholesStrengthTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight; strength::String="Mild")

const placements = Ahorn.PlacementDict(
    "Black Hole Strength (All Black Holes) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        AllBlackholesStrengthTrigger,
        "rectangle"
    )
)

function Ahorn.editingOptions(trigger::AllBlackholesStrengthTrigger)
    return Dict{String, Any}(
        "strength" => Maple.black_hole_trigger_strengths
    )
end

end
