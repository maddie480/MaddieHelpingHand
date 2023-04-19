module MaxHelpingHandFancyTextTutorial

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/FancyTextTutorial" FancyTextTutorial(x::Integer, y::Integer, birdId::String="",
    dialogId::String="TUTORIAL_DREAMJUMP", textScale::Number=1.0, direction::String="Down", onlyOnce::Bool=false)

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Fancy Text Tutorial (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        FancyTextTutorial
    )
)

Ahorn.editingOptions(entity::FancyTextTutorial) = return Dict{String, Any}(
    "direction" => String["Up", "Down", "Left", "Right", "None"]
)

sprite = "ahorn/MaxHelpingHand/greyscale_birb"

function Ahorn.selection(entity::FancyTextTutorial)
    x, y = Ahorn.position(entity)
    scaleX = -1

    return Ahorn.getSpriteRectangle(sprite, x, y, sx=scaleX, jx=0.5, jy=1.0)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FancyTextTutorial, room::Maple.Room)
    scaleX = -1

    Ahorn.drawSprite(ctx, sprite, 0, 0, sx=scaleX, jx=0.5, jy=1.0)
end

end
