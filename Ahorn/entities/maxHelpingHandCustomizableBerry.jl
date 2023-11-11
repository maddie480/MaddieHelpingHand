module MaxHelpingHandCustomizableBerry

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomizableBerry" CustomizableBerry(x::Integer, y::Integer,
    checkpointID::Integer=-1, order::Integer=-1, visibleIfFlag::String="", strawberrySprite::String="strawberry", ghostberrySprite::String="ghostberry",
    strawberryPulseSound::String="event:/game/general/strawberry_pulse", strawberryBlueTouchSound::String="event:/game/general/strawberry_blue_touch",
    strawberryTouchSound::String="event:/game/general/strawberry_touch", strawberryGetSound::String="event:/game/general/strawberry_get", countTowardsTotal::Bool=true,
    pulseEnabled::Bool=true, particleColor1::String="FF8563", particleColor2::String="FFF4A8", ghostParticleColor1::String="6385FF", ghostParticleColor2::String="72F0FF")

const placements = Ahorn.PlacementDict(
    "Customizable Berry (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CustomizableBerry,
        "point"
    )
)

Ahorn.editingOrder(entity::CustomizableBerry) = String["x", "y", "strawberrySprite", "ghostberrySprite", "strawberryPulseSound", "strawberryTouchSound", "strawberryBlueTouchSound", "strawberryGetSound",
    "particleColor1", "particleColor2", "ghostParticleColor1", "ghostParticleColor2"]
Ahorn.editingIgnored(entity::CustomizableBerry, multiple::Bool=false) = String["countTowardsTotal"]

sprite = "collectables/strawberry/normal00"

function Ahorn.selection(entity::CustomizableBerry)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomizableBerry, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

end