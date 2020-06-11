module MaxHelpingHandBlackholeCustomColors

using ..Ahorn, Maple

@mapdef Effect "MaxHelpingHand/BlackholeCustomColors" BlackholeCustomColors(only::String="*", exclude::String="", colorsMild::String="6e3199,851f91,3026b0", colorsWild::String="ca4ca7,b14cca,ca4ca7",
    bgColorInner::String="000000", bgColorOuterMild::String="512a8b", bgColorOuterWild::String="bd2192")

placements = BlackholeCustomColors

function Ahorn.canFgBg(effect::BlackholeCustomColors)
    return true, true
end

end
