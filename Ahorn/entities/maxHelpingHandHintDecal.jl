module MaxHelpingHandHintDecal

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/HintDecal" HintDecal(x::Integer, y::Integer, texture::String="1-forsakencity/sign_you_can_go_up", scaleX::Number=1.0, scaleY::Number=1.0, foreground::Bool=false)

const placements = Ahorn.PlacementDict(
    "Hint Decal (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        HintDecal
    ),
    "Custom Bird Tutorial (Show Hints) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        Maple.EverestCustomBird,
        "rectangle",
        Dict{String,Any}(
            "info" => "MAXHELPINGHAND_SHOWHINTS",
            "controls" => "ShowHints"
        )
    )
)

# This code handling decal selection, rendering and properties window was borrowed from a Ahorn plugin by lilybeevee

function Ahorn.selection(entity::HintDecal)
    x, y = Ahorn.position(entity)
    sprite = "decals/$(get(entity, "texture", ""))"
    scaleX = get(entity, "scaleX", 1.0)
    scaleY = get(entity, "scaleY", 1.0)

    return Ahorn.getSpriteRectangle(sprite, x, y, sx=scaleX, sy=scaleY)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HintDecal, room::Maple.Room)
    sprite = "decals/$(get(entity, "texture", ""))"
    scaleX = get(entity, "scaleX", 1.0)
    scaleY = get(entity, "scaleY", 1.0)

    Ahorn.drawSprite(ctx, sprite, 0, 0, sx=scaleX, sy=scaleY)
end

animationRegex = r"\D+0*?$"
filterAnimations(s::String) = occursin(animationRegex, s)

function decalTextures()
    Ahorn.loadChangedExternalSprites!()
    textures = Ahorn.spritesToDecalTextures(Ahorn.getAtlas("Gameplay"))
    filter!(filterAnimations, textures)
    sort!(textures)
    return textures
end

Ahorn.editingOptions(entity::HintDecal) = Dict{String, Any}(
    "texture" => decalTextures()
)

end
