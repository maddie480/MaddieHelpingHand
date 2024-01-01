module MaxHelpingHandSpinnerBreakingBall

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/SpinnerBreakingBall" SpinnerBreakingBall(x::Integer, y::Integer, color::String="Blue",
    spritePath::String="MaxHelpingHand/spinner_breaking_ball_placeholder", startFloating::Bool=false, rainbowTinting::Bool=true)
@mapdef Entity "MaxHelpingHand/SpinnerBreakingBallFrostHelper" SpinnerBreakingBallFrost(x::Integer, y::Integer, color::String="FFFFFF",
    spritePath::String="MaxHelpingHand/spinner_breaking_ball_placeholder", startFloating::Bool=false)

const colors = String["Blue", "Red", "Purple", "Rainbow"]
const sprites = String[
    "MaxHelpingHand/spinner_breaking_ball_placeholder",
    "MaxHelpingHand/SpinnerBreakBallBlue",
    "MaxHelpingHand/SpinnerBreakBallRed",
    "MaxHelpingHand/SpinnerBreakBallPurple",
    "MaxHelpingHand/SpinnerBreakBallRainbow"
];

const placementDict = Dict{String, Ahorn.EntityPlacement}(
    "Spinner Breaking Ball ($(color)) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        SpinnerBreakingBall,
        "point",
        Dict{String,Any}(
            "color" => color,
            "spritePath" => "MaxHelpingHand/SpinnerBreakBall" * uppercase(color[1]) * color[2:end],
            "rainbowTinting" => (color == "Rainbow")
        )
    ) for color in colors
)

placementDict["Spinner Breaking Ball (Custom) (Maddie's Helping Hand + Frost Helper)"] =  Ahorn.EntityPlacement(
    SpinnerBreakingBallFrost,
    "point",
    Dict{String,Any}(
        "spritePath" => "MaxHelpingHand/SpinnerBreakBallBlue"
    )
)

const placements = Ahorn.PlacementDict(placementDict)

Ahorn.editingOptions(entity::SpinnerBreakingBall) = Dict{String, Any}(
    "color" => colors,
    "spritePath" => sprites
)
Ahorn.editingOptions(entity::SpinnerBreakingBallFrost) = Dict{String, Any}(
    "spritePath" => sprites
)

const balls = Union{SpinnerBreakingBall, SpinnerBreakingBallFrost}

function Ahorn.selection(entity::balls)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(entity.spritePath, x, y - 10)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::balls, room::Maple.Room) = Ahorn.drawSprite(ctx, entity.spritePath, 0, -10)

end
