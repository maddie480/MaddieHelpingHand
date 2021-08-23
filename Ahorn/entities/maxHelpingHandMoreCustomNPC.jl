module MaxHelpingHandMoreCustomNPC

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/MoreCustomNPC" MoreCustomNPC(x::Integer, y::Integer, sprite::String="player/idle", spriteRate::Int=1, dialogId::String="", onlyOnce::Bool=true, endLevel::Bool=false,
	flipX::Bool=false, flipY::Bool=false, approachWhenTalking::Bool=false, approachDistance::Int=16, indicatorOffsetX::Int=0, indicatorOffsetY::Int=0,
	frames::String="", onlyIfFlag::String="", setFlag::String="", nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

@mapdef Entity "MaxHelpingHand/CustomNPCSprite" CustomNPCSprite(x::Integer, y::Integer, dialogId::String="", onlyOnce::Bool=true, endLevel::Bool=false,
	flipX::Bool=false, flipY::Bool=false, approachWhenTalking::Bool=false, approachDistance::Int=16, indicatorOffsetX::Int=0, indicatorOffsetY::Int=-16,
	spriteName::String="bird", onlyIfFlag::String="", setFlag::String="", nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const npcUnion = Union{MoreCustomNPC, CustomNPCSprite}

const placements = Ahorn.PlacementDict(
    # Base placements
    "More Custom NPC\n(Everest + max480's Helping Hand)" => Ahorn.EntityPlacement(
        MoreCustomNPC
    ),
    "Custom NPC (from XML)\n(Everest + max480's Helping Hand)" => Ahorn.EntityPlacement(
        CustomNPCSprite
    ),

    # Presets for Granny and an invisible NPC that don't actually require More Custom NPC but are here for convenience/completeness
    "Custom NPC (Granny) (Everest)" => Ahorn.EntityPlacement(
        Maple.EverestCustomNPC,
        "point",
        Dict{String,Any}(
            "sprite" => "oldlady/idle",
            "spriteRate" => 7,
            "indicatorOffsetY" => -2
        )
    ),
    "Custom NPC (Invisible) (Everest)" => Ahorn.EntityPlacement(
        Maple.EverestCustomNPC,
        "point",
        Dict{String,Any}(
            "sprite" => "",
            "indicatorOffsetY" => -16
        )
    ),

    # Presets for Theo, Oshiro and Badeline Boss that do require More Custom NPC
    "More Custom NPC (Theo)\n(Everest + max480's Helping Hand)" => Ahorn.EntityPlacement(
        MoreCustomNPC,
        "point",
        Dict{String,Any}(
            "sprite" => "theo/theo",
            "frames" => "0-9",
            "spriteRate" => 10,
            "indicatorOffsetY" => -5
        )
    ),
    "More Custom NPC (Oshiro)\n(Everest + max480's Helping Hand)" => Ahorn.EntityPlacement(
        MoreCustomNPC,
        "point",
        Dict{String,Any}(
            "sprite" => "oshiro/oshiro",
            "frames" => "31-41",
            "spriteRate" => 12,
            "indicatorOffsetY" => -10
        )
    ),
    "More Custom NPC (Badeline Boss)\n(Everest + max480's Helping Hand)" => Ahorn.EntityPlacement(
        MoreCustomNPC,
        "point",
        Dict{String,Any}(
            "sprite" => "badelineBoss/boss",
            "frames" => "0-23",
            "spriteRate" => 17,
            "indicatorOffsetY" => -5
        )
    )
)

Ahorn.nodeLimits(entity::npcUnion) = 0, 2

function getSpriteName(entity::MoreCustomNPC)
    spriteName = get(entity.data, "sprite", "")

    if spriteName == "oshiro/oshiro"
        return "characters/oshiro/oshiro31"
    end

    return "characters/$(spriteName)00"
end

function getSpriteName(entity::CustomNPCSprite)
    return "ahorn/MaxHelpingHand/custom_npc_xml"
end

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::npcUnion)
    x, y = Ahorn.position(entity)

    spriteName = getSpriteName(entity)
    sprite = Ahorn.getSprite(spriteName, "Gameplay")

    nodes = get(entity.data, "nodes", Tuple{Int, Int}[])
    offsetCenterY = - floor(Int, sprite.height / 2)

    if length(nodes) == 2
        n1x, n1y = Int.(nodes[1])
        n2x, n2y = Int.(nodes[2])
        Ahorn.drawRectangle(ctx, n1x, n1y, n2x - n1x + 8, n2y - n1y + 8, Ahorn.colors.trigger_fc, Ahorn.colors.trigger_bc)
    end

    for node in nodes
        nx, ny = Int.(node)
        Ahorn.drawRectangle(ctx, nx, ny, 8, 8, Ahorn.colors.trigger_fc, Ahorn.colors.trigger_bc)
    end
end

function Ahorn.selection(entity::npcUnion)
    x, y = Ahorn.position(entity)

    scaleX, scaleY = get(entity.data, "flipX", false) ? -1 : 1, get(entity.data, "flipY", false) ? -1 : 1

    spriteName = getSpriteName(entity)
    sprite = Ahorn.getSprite(spriteName, "Gameplay")

    if sprite.width == 0 || sprite.height == 0
        res = Ahorn.Rectangle[Ahorn.Rectangle(x - 4, y - 4, 8, 8)]
    else
        res = Ahorn.Rectangle[Ahorn.getSpriteRectangle(spriteName, x, y, jx=0.5, jy=1.0, sx=scaleX, sy=scaleY)]
    end
    
    for node in get(entity.data, "nodes", ())
        nx, ny = Int.(node)
        push!(res, Ahorn.Rectangle(nx, ny, 8, 8))
    end

    return res
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::npcUnion, room::Maple.Room)
    scaleX, scaleY = get(entity.data, "flipX", false) ? -1 : 1, get(entity.data, "flipY", false) ? -1 : 1
    spriteName = getSpriteName(entity)

    Ahorn.drawSprite(ctx, spriteName, 0, 0, jx=0.5, jy=1.0, sx=scaleX, sy=scaleY)
end

end