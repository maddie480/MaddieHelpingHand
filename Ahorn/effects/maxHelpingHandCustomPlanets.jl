module MaxHelpingHandCustomPlanets

using ..Ahorn, Maple

@mapdef Effect "MaxHelpingHand/CustomPlanets" CustomPlanets(only::String="*", exclude::String="", count::Number=32, color::String="", scrollx::Number=1.0, scrolly::Number=1.0,
    speedx::Number=0.0, speedy::Number=0.0, directory::String="MaxHelpingHand/customplanets/bigstars", animationDelay::Number=0.1, fadex::String="", fadey::String="")

placements = CustomPlanets

function Ahorn.canFgBg(effect::CustomPlanets)
    return true, true
end

function Ahorn.editingOptions(effect::CustomPlanets)
    return Dict{String, Any}(
        "directory" => String[
            "MaxHelpingHand/customplanets/bigstars",
            "MaxHelpingHand/customplanets/smallstars",
            "MaxHelpingHand/customplanets/bigplanets",
            "MaxHelpingHand/customplanets/smallplanets",
            "MaxHelpingHand/customplanets/rainbowstars"
        ]
    )
end

end
