module MaxHelpingHandHeatWaveNoColorGrade

using ..Ahorn, Maple

@mapdef Effect "MaxHelpingHand/HeatWaveNoColorGrade" HeatWaveNoColorGrade(only::String="*", exclude::String="", controlColorGradeWhenActive::Bool=false)

placements = HeatWaveNoColorGrade

function Ahorn.canFgBg(effect::HeatWaveNoColorGrade)
    return true, true
end

end
