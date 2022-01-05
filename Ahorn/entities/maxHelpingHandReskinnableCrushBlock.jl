module MaxHelpingHandReskinnableCrushBlock

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/ReskinnableCrushBlock" ReskinnableCrushBlock(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
	axes::String="both", chillout::Bool=false, spriteDirectory::String="objects/crushblock", fillColor::String="62222b", crushParticleColor1::String="ff66e2", crushParticleColor2::String="68fcff",
    activateParticleColor1::String="5fcde4", activateParticleColor2::String="ffffff", soundDirectory::String="event:/game/06_reflection")

const placements = Ahorn.PlacementDict(
    "Kevin (Both, Reskinnable) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableCrushBlock,
        "rectangle"
    ),
    "Kevin (Vertical, Reskinnable) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableCrushBlock,
        "rectangle",
        Dict{String, Any}(
            "axes" => "vertical"
        )
    ),
    "Kevin (Horizontal, Reskinnable) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableCrushBlock,
        "rectangle",
        Dict{String, Any}(
            "axes" => "horizontal"
        )
    ),
)

Ahorn.editingOptions(entity::ReskinnableCrushBlock) = Dict{String, Any}(
    "axes" => Maple.kevin_axes
)

Ahorn.minimumSize(entity::ReskinnableCrushBlock) = 24, 24
Ahorn.resizable(entity::ReskinnableCrushBlock) = true, true

Ahorn.selection(entity::ReskinnableCrushBlock) = Ahorn.getEntityRectangle(entity)

# Todo - Use randomness to decide on Kevin border
function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableCrushBlock, room::Maple.Room)
    kevinColor = Ahorn.argb32ToRGBATuple(parse(Int, get(entity.data, "fillColor", "62222b"), base=16))[1:3] ./ 255
    spriteDirectory = get(entity.data, "spriteDirectory", "objects/crushblock")

    frameImage = Dict{String, String}(
        "none" => "$(spriteDirectory)/block00",
        "horizontal" => "$(spriteDirectory)/block01",
        "vertical" => "$(spriteDirectory)/block02",
        "both" => "$(spriteDirectory)/block03"
    )

    smallFace = "$(spriteDirectory)/idle_face"
    giantFace = "$(spriteDirectory)/giant_block00"

    axes = lowercase(get(entity.data, "axes", "both"))
    chillout = get(entity.data, "chillout", false)

    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    giant = height >= 48 && width >= 48 && chillout
    face = giant ? giantFace : smallFace
    frame = frameImage[lowercase(axes)]
    faceSprite = Ahorn.getSprite(face, "Gameplay")

    tilesWidth = div(width, 8)
    tilesHeight = div(height, 8)

    Ahorn.drawRectangle(ctx, 2, 2, width - 4, height - 4, kevinColor)
    Ahorn.drawImage(ctx, faceSprite, div(width - faceSprite.width, 2), div(height - faceSprite.height, 2))

    for i in 2:tilesWidth - 1
        Ahorn.drawImage(ctx, frame, (i - 1) * 8, 0, 8, 0, 8, 8)
        Ahorn.drawImage(ctx, frame, (i - 1) * 8, height - 8, 8, 24, 8, 8)
    end

    for i in 2:tilesHeight - 1
        Ahorn.drawImage(ctx, frame, 0, (i - 1) * 8, 0, 8, 8, 8)
        Ahorn.drawImage(ctx, frame, width - 8, (i - 1) * 8, 24, 8, 8, 8)
    end

    Ahorn.drawImage(ctx, frame, 0, 0, 0, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, 0, 24, 0, 8, 8)
    Ahorn.drawImage(ctx, frame, 0, height - 8, 0, 24, 8, 8)
    Ahorn.drawImage(ctx, frame, width - 8, height - 8, 24, 24, 8, 8)
end

end