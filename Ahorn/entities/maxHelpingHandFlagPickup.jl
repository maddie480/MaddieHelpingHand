module MaxHelpingHandFlagPickup

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/FlagPickup" FlagPickup(x::Integer, y::Integer,
    appearOnFlag::String="", flagOnPickup::String="", collectFlag::String="", spriteName::String="MaxHelpingHand_FlagPickup_Flag",
    collectSound::String="event:/game/general/seed_touch", allowRespawn::Bool=false)

const placements = Ahorn.PlacementDict(
    "Flag Pickup (max480's Helping Hand)" => Ahorn.EntityPlacement(
        FlagPickup
    )
)

Ahorn.editingOptions(entity::FlagPickup) = Dict{String, Any}(
    "spriteName" => String["MaxHelpingHand_FlagPickup_Coin", "MaxHelpingHand_FlagPickup_Flag"]
)

function Ahorn.selection(entity::FlagPickup)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 8, y - 8, 16, 16)
end

function getSpritePath(entity::FlagPickup)
    if entity.spriteName == "MaxHelpingHand_FlagPickup_Coin"
        return "MaxHelpingHand/flagpickup/Coin/Coin0"
    else
        return "MaxHelpingHand/flagpickup/Flag/Flag0"
    end
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlagPickup, room::Maple.Room)
    Ahorn.drawSprite(ctx, getSpritePath(entity), 0, 0)
end

end
