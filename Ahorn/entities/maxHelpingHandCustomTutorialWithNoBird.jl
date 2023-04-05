module MaxHelpingHandCustomTutorialWithNoBird

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomTutorialWithNoBird" CustomTutorialWithNoBird(x::Integer, y::Integer, birdId::String="", onlyOnce::Bool=false,
    info::String="TUTORIAL_DREAMJUMP", controls::String="DownRight,+,Dash,tinyarrow,Jump", direction::String="Down")

using ..Ahorn, Maple

const placements = Ahorn.PlacementDict(
    "Custom Tutorial with No Bird (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CustomTutorialWithNoBird
    )
)

Ahorn.editingOptions(entity::CustomTutorialWithNoBird) = return Dict{String, Any}(
    "info" => Maple.everest_bird_tutorial_tutorials,
    "direction" => String["Up", "Down", "Left", "Right", "None"]
)

sprite = "ahorn/MaxHelpingHand/greyscale_birb"

function Ahorn.selection(entity::CustomTutorialWithNoBird)
    x, y = Ahorn.position(entity)
    scaleX = -1

    return Ahorn.getSpriteRectangle(sprite, x, y, sx=scaleX, jx=0.5, jy=1.0)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomTutorialWithNoBird, room::Maple.Room)
    scaleX = -1

    Ahorn.drawSprite(ctx, sprite, 0, 0, sx=scaleX, jx=0.5, jy=1.0)
end

end
