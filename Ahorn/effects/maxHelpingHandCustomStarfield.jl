module MaxHelpingHandCustomStarfield

using ..Ahorn, Maple

@mapdef Effect "MaxHelpingHand/CustomStarfield" CustomStarfield(
    only::String="*", exclude::String="",
    paths::String="starfield", colors::String="ffffff", alphas::String="1",
    shuffle::Bool=true,
    speed::Number=1.0,
    fadex::String="", fadey::String="",
    scrollx::Number=1.0, scrolly::Number=1.0)

placements = CustomStarfield

function Ahorn.canFgBg(effect::CustomStarfield)
    return true, true
end

end