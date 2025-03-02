module MaxHelpingHandSnowCustomColors

using ..Ahorn, Maple

@mapdef Effect "MaxHelpingHand/SnowCustomColors" SnowCustomColors(only::String="*", exclude::String="", colors::String="FF0000,00FF00,0000FF", speedMin::Number=40.0, speedMax::Number=100.0, alpha::Number=1.0, particleCount::Integer=60)

placements = SnowCustomColors

function Ahorn.canFgBg(effect::SnowCustomColors)
    return true, true
end

end
