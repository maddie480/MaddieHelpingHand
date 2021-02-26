module MaxHelpingHandCustomizableCrumblePlatform

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomizableCrumblePlatform" CustomizableCrumblePlatform(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth,
    texture::String="default", oneUse::Bool=false, respawnDelay::Number=2.0, grouped::Bool=false, minCrumbleDurationOnTop::Number=0.2,
    maxCrumbleDurationOnTop::Number=0.6, crumbleDurationOnSide::Number=1.0, outlineTexture::String="objects/crumbleBlock/outline")

const placements = Ahorn.PlacementDict(
    "Crumble Blocks ($(uppercasefirst(texture)), Customizable)\n(max480's Helping Hand)" => Ahorn.EntityPlacement(
        CustomizableCrumblePlatform,
        "rectangle",
        Dict{String, Any}(
            "texture" => texture
        )
    ) for texture in Maple.crumble_block_textures
)

Ahorn.editingOptions(entity::CustomizableCrumblePlatform) = Dict{String, Any}(
    "texture" => Maple.crumble_block_textures
)

Ahorn.editingOrder(entity::CustomizableCrumblePlatform) = String["x", "y", "width", "crumbleDurationOnSide", "minCrumbleDurationOnTop", "maxCrumbleDurationOnTop"]

Ahorn.minimumSize(entity::CustomizableCrumblePlatform) = 8, 0
Ahorn.resizable(entity::CustomizableCrumblePlatform) = true, false

function Ahorn.selection(entity::CustomizableCrumblePlatform)
    x, y = Ahorn.position(entity)
    width = Int(get(entity.data, "width", 8))

    return Ahorn.Rectangle(x, y, width, 8)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomizableCrumblePlatform, room::Maple.Room)
    texture = get(entity.data, "texture", "default")
    texture = "objects/crumbleBlock/$texture"

    # Values need to be system specific integer
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 8))
    tilesWidth = div(width, 8)

    Ahorn.Cairo.save(ctx)

    Ahorn.rectangle(ctx, 0, 0, width, 8)
    Ahorn.clip(ctx)

    for i in 0:ceil(Int, tilesWidth / 4)
        Ahorn.drawImage(ctx, texture, 32 * i, 0, 0, 0, 32, 8)
    end

    Ahorn.restore(ctx)
end

end
