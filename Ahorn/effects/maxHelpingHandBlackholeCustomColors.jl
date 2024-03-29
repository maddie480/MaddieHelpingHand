﻿module MaxHelpingHandBlackholeCustomColors

using ..Ahorn, Maple

@mapdef Effect "MaxHelpingHand/BlackholeCustomColors" BlackholeCustomColors(only::String="*", exclude::String="", colorsMild::String="6e3199,851f91,3026b0", colorsWild::String="ca4ca7,b14cca,ca4ca7",
    bgColorInner::String="000000", bgColorOuterMild::String="512a8b", bgColorOuterWild::String="bd2192", alpha::Number=1.0, affectedByWind::Bool=true, fadex::String="", fadey::String="",
    direction::Number=1.0, additionalWindX::Number=0.0, additionalWindY::Number=0.0, bgAlphaInner::Number=1.0, bgAlphaOuter::Number=1.0, fgAlpha::Number=1.0, invertedRendering::Bool=false,
    texture::String="", particleTexture::String="", particleTextureCount::Integer=1)

placements = BlackholeCustomColors

function Ahorn.canFgBg(effect::BlackholeCustomColors)
    return true, true
end

Ahorn.editingOrder(effect::BlackholeCustomColors) = String[
    "only", "exclude", "flag", "notflag",
    "colorsMild", "colorsWild", "fadex", "fadey",
    "bgColorInner", "bgColorOuterMild", "bgColorOuterWild", "tag",
    "bgAlphaInner", "bgAlphaOuter", "fgAlpha", "alpha"
]

end
