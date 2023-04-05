module MaxHelpingHandMultiRoomStrawberrySeed

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/MultiRoomStrawberrySeed" MultiRoomStrawberrySeed(x::Integer, y::Integer,
    strawberryName::String="multi_room_strawberry", sprite::String="MaxHelpingHand/miniberry/miniberry", ghostSprite::String="MaxHelpingHand/miniberry/ghostminiberry", index::Int=-1, displaySeedCount::Bool=false)

const bundledSprites = String["strawberry/seed", "MaxHelpingHand/miniberry/miniberry"]
const bundledGhostSprites = String["ghostberry/seed", "MaxHelpingHand/miniberry/ghostminiberry"]

const placements = Ahorn.PlacementDict(
    "Multi-Room Strawberry Seed (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        MultiRoomStrawberrySeed
    )
)


Ahorn.editingOptions(entity::MultiRoomStrawberrySeed) = Dict{String,Any}(
    "sprite" => bundledSprites,
    "ghostSprite" => bundledGhostSprites
)

function Ahorn.selection(entity::MultiRoomStrawberrySeed)
    x, y = Ahorn.position(entity)
    sprite = "collectables/" * get(entity.data, "sprite", "strawberry/seed") * "00"

    return Ahorn.getSpriteRectangle(sprite, x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MultiRoomStrawberrySeed, room::Maple.Room)
    sprite = "collectables/" * get(entity.data, "sprite", "strawberry/seed") * "00"

    Ahorn.drawSprite(ctx, sprite, 0, 0)
end

end
