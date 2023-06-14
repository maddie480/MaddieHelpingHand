module MaxHelpingHandFlagBreakerBox

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/FlagBreakerBox" FlagBreakerBox(x::Integer, y::Integer,
    flag::String="", health::Int=2, floaty::Bool=true, bouncy::Bool=true, sprite::String="breakerBox", flipX::Bool=false,
    music::String="", music_progress::Int=-1, music_session::Bool=false, surfaceIndex::Int=9, color::String="fffc75", color2::String="6bffff", refill::Bool=true)

const placements = Ahorn.PlacementDict(
    "Flag Breaker Box (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        FlagBreakerBox
    )
)

sprite = "objects/breakerBox/Idle00"

Ahorn.editingOptions(entity::FlagBreakerBox) = Dict{String, Any}(
    "surfaceIndex" => Maple.tileset_sound_ids
)

function Ahorn.selection(entity::FlagBreakerBox)
    x, y = Ahorn.position(entity)
    scaleX = get(entity, "flipX", false) ? -1 : 1
    justificationX = get(entity, "flipX", false) ? 0.75 : 0.25

    return Ahorn.getSpriteRectangle(sprite, x, y, sx=scaleX, jx=justificationX, jy=0.25)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlagBreakerBox, room::Maple.Room)
    scaleX = get(entity, "flipX", false) ? -1 : 1
    justificationX = get(entity, "flipX", false) ? 0.75 : 0.25

    Ahorn.drawSprite(ctx, sprite, 0, 0, sx=scaleX, jx=justificationX, jy=0.25)
end

end
