module MaxHelpingHandNorthernLightsCustomColors

using ..Ahorn, Maple

@mapdef Effect "MaxHelpingHand/NorthernLightsCustomColors" NorthernLightsCustomColors(only::String="*", exclude::String="", gradientColor1::String="020825", gradientColor2::String="170c2f",
    colors::String="2de079,62f4f6,45bc2e,3856f0", displayBackground::Bool=true)

placements = NorthernLightsCustomColors

function Ahorn.canFgBg(effect::NorthernLightsCustomColors)
    return true, true
end

end
