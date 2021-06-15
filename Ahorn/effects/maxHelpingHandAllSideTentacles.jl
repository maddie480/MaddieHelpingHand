module MaxHelpingHandAllSideTentacles

using ..Ahorn, Maple

@mapdef Effect "MaxHelpingHand/AllSideTentacles" AllSideTentacles(only::String="*", exclude::String="", side::String="Right", color::String="", offset::Number=0)

placements = AllSideTentacles

function Ahorn.canFgBg(effect::AllSideTentacles)
    return true, true
end

function Ahorn.editingOptions(effect::AllSideTentacles)
    return Dict{String, Any}(
        "side" => String["Top", "Bottom", "Left", "Right"]
    )
end

end
