module MaxHelpingHandSidewaysLava

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/SidewaysLava" SidewaysLava(x::Integer, y::Integer,
    intro::Bool=false, lavaMode::String="LeftToRight", speedMultiplier::Number=1.0,
    hotSurfaceColor::String="ff8933", hotEdgeColor::String="f25e29", hotCenterColor::String="d01c01",
    coldSurfaceColor::String="33ffe7", coldEdgeColor::String="4ca2eb", coldCenterColor::String="0151d0",
    sound::String="event:/game/09_core/rising_threat", forceCoreMode::String="None")

const placements = Ahorn.PlacementDict(
    "Sideways Lava (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        SidewaysLava
    )
)

Ahorn.editingOptions(entity::SidewaysLava) = Dict{String, Any}(
    "lavaMode" => String["LeftToRight", "RightToLeft", "Sandwich"],
    "forceCoreMode" => String["None", "Cold", "Hot"]
)

function Ahorn.selection(entity::SidewaysLava)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SidewaysLava, room::Maple.Room)
    Ahorn.Cairo.save(ctx)

    lavaMode = get(entity.data, "lavaMode", "LeftToRight")

    Ahorn.rotate(ctx, (lavaMode == "RightToLeft" ? -1 : 1) * pi / 2)
    Ahorn.drawImage(ctx, (lavaMode == "Sandwich" ? Ahorn.Assets.lavaSanwitch : Ahorn.Assets.risingLava), -12, -12)

    Ahorn.Cairo.restore(ctx);
end

end
