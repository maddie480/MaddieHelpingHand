module MaxHelpingHandBeeFireball

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/BeeFireball" BeeFireball(x::Integer, y::Integer, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[], amount::Integer=1, offset::Number=0.0, speed::Number=1.0)

function fireballFinalizer(entity::BeeFireball)
    x, y = Ahorn.position(entity)
    entity.data["nodes"] = [(x + 16, y)]
end

const placements = Ahorn.PlacementDict(
    "Bee Fireball (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        BeeFireball,
        "point",
        Dict{String, Any}(
            "amount" => 3
        ),
        fireballFinalizer
    )
)

Ahorn.nodeLimits(entity::BeeFireball) = 1, -1

sprite = "objects/MaxHelpingHand/beeFireball/beefireball00"

function Ahorn.selection(entity::BeeFireball)
    nodes = get(entity.data, "nodes", ())
    x, y = Ahorn.position(entity)

    res = Ahorn.Rectangle[Ahorn.Rectangle(x - 12, y - 12, 24, 24)]

    for node in nodes
        nx, ny = Int.(node)

        push!(res, Ahorn.Rectangle(nx - 12, ny - 12, 24, 24))
    end

    return res
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::BeeFireball)
    px, py = Ahorn.position(entity)

    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)

        Ahorn.drawArrow(ctx, px, py, nx, ny, Ahorn.colors.selection_selected_fc, headLength=6)
        Ahorn.drawSprite(ctx, sprite, nx - 2, ny + 3)

        px, py = nx, ny
    end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::BeeFireball, room::Maple.Room)
    x, y = Ahorn.position(entity)
    Ahorn.drawSprite(ctx, sprite, x - 2, y + 3)
end

end
