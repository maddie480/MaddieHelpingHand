module MaxHelpingHandSnowCustomColors

using ..Ahorn, Maple

@mapdef Effect "MaxHelpingHand/SnowCustomColors" SnowCustomColors(only::String="*", exclude::String="", colors::String="FF0000,00FF00,0000FF", speedMin::Number=40.0, speedMax::Number=100.0, alpha::Number=1.0, particleCount::Integer=60,
    angle::Number=0.0, sineAmplitudeMult::Number=0.2, texturePath::String="", scrollx::Number=1.0, scrolly::Number=1.0, fadex::String="", fadey::String="")

placements = SnowCustomColors

function Ahorn.canFgBg(effect::SnowCustomColors)
    return true, true
end

end
