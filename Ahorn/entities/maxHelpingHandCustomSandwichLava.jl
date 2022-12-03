module MaxHelpingHandCustomSandwichLava

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomSandwichLava" CustomSandwichLava(x::Integer, y::Integer,
    direction::String="CoreModeBased", speed::Number=20.0, sandwichGap::Number=160.0,
    hotSurfaceColor::String="ff8933", hotEdgeColor::String="f25e29", hotCenterColor::String="d01c01",
    coldSurfaceColor::String="33ffe7", coldEdgeColor::String="4ca2eb", coldCenterColor::String="0151d0")

const directions = String["AlwaysUp", "AlwaysDown", "CoreModeBased"]

const placements = Ahorn.PlacementDict(
    "Sandwich Lava (Customizable) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        CustomSandwichLava
    )
)

Ahorn.editingOptions(entity::CustomSandwichLava) = Dict{String, Any}(
    "direction" => directions
)

function Ahorn.selection(entity::CustomSandwichLava)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomSandwichLava, room::Maple.Room)
    direction = get(entity.data, "direction", "CoreModeBased")

    if direction == "AlwaysUp"
        Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/lava_sandwich_up", 0, 0)
    elseif direction == "AlwaysDown"
        Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/lava_sandwich_down", 0, 0)
    else
        Ahorn.drawImage(ctx, Ahorn.Assets.lavaSanwitch, -12, -12)
    end
end

end