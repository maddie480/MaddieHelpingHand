module MaxHelpingHandSpinnerBreakingBall

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/SpinnerBreakingBall" SpinnerBreakingBall(x::Integer, y::Integer, color::String="Blue", spritePath::String="MaxHelpingHand/spinner_breaking_ball_placeholder", startFloating::Bool=false)

const colors = String["Blue", "Red", "Purple", "Rainbow"]

const placements = Ahorn.PlacementDict(
    "Spinner Breaking Ball ($(color)) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        SpinnerBreakingBall,
        "point",
        Dict{String,Any}(
            "color" => color
        )
    ) for color in colors
)

Ahorn.editingOptions(entity::SpinnerBreakingBall) = Dict{String, Any}(
    "color" => colors
)

function Ahorn.selection(entity::SpinnerBreakingBall)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(entity.spritePath, x, y - 10)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpinnerBreakingBall, room::Maple.Room) = Ahorn.drawSprite(ctx, entity.spritePath, 0, -10)

end
