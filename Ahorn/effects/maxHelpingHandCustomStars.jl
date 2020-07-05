module MaxHelpingHandCustomStars

using ..Ahorn, Maple

@mapdef Effect "MaxHelpingHand/CustomStars" CustomStars(only::String="*", exclude::String="", spriteDirectory::String="bgs/02/stars", disableTinting::Bool=false)

placements = CustomStars

function Ahorn.canFgBg(effect::CustomStars)
    return true, true
end

end
