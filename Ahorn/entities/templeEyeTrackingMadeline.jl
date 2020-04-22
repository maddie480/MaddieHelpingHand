module MaxHelpingHandTempleEyeTrackingMadeline

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Temple Eye (Small, Follow Madeline) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        Maple.TempleEye,
        "point",
        Dict{String, Any}(
            "followMadeline" => true
        )
    )
)

end
