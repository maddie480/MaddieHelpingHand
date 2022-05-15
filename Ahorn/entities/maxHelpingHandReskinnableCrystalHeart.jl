module MaxHelpingHandReskinnableCrystalHeart

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/ReskinnableCrystalHeart" ReskinnableCrystalHeart(x::Integer, y::Integer, fake::Bool=false, removeCameraTriggers::Bool=false, fakeHeartDialog::String="CH9_FAKE_HEART",
    keepGoingDialog::String="CH9_KEEP_GOING", sprite::String="", ghostSprite::String="", particleColor::String="", flagOnCollect::String="", flagInverted::Bool=false, disableGhostSprite::Bool=false)

const placements = Ahorn.PlacementDict(
    "Crystal Heart (Reskinnable) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ReskinnableCrystalHeart
    ),
)

Ahorn.editingOptions(entity::ReskinnableCrystalHeart) = Dict{String, Any}(
    "sprite" => String["", "heartgem0", "heartgem1", "heartgem2", "heartgem3"],
    "ghostSprite" => String["", "heartgem0", "heartgem1", "heartgem2", "heartgem3"]
)

function getSprite(entity::ReskinnableCrystalHeart)
    if entity.sprite == "heartgem1"
        return "collectables/heartGem/1/00"
    elseif entity.sprite == "heartgem2"
        return "collectables/heartGem/2/00"
    elseif entity.sprite == "heartgem3"
        return "collectables/heartGem/3/00"
    else
        return "collectables/heartGem/0/00"
    end
end

function Ahorn.selection(entity::ReskinnableCrystalHeart)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(getSprite(entity), x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ReskinnableCrystalHeart, room::Maple.Room) = Ahorn.drawSprite(ctx, getSprite(entity), 0, 0)

end
