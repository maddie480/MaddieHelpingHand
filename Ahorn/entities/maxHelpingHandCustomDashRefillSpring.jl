﻿module MaxHelpingHandCustomDashRefillSpring

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomDashRefillSpring" CustomDashRefillSpring(x::Integer, y::Integer, spriteDirectory::String="objects/MaxHelpingHand/twoDashRefillSpring", playerCanUse::Bool=true,
    ignoreLighting::Bool=false, refillStamina::Bool=true, dashCount::Int=2, dashCountCap::Int=2, mode::String="Set")
@mapdef Entity "MaxHelpingHand/CustomDashRefillSpringRight" CustomDashRefillSpringRight(x::Integer, y::Integer, spriteDirectory::String="objects/MaxHelpingHand/twoDashRefillSpring",
    ignoreLighting::Bool=false, refillStamina::Bool=true, dashCount::Int=2, dashCountCap::Int=2, mode::String="Set")
@mapdef Entity "MaxHelpingHand/CustomDashRefillSpringLeft" CustomDashRefillSpringLeft(x::Integer, y::Integer, spriteDirectory::String="objects/MaxHelpingHand/twoDashRefillSpring",
    ignoreLighting::Bool=false, refillStamina::Bool=true, dashCount::Int=2, dashCountCap::Int=2, mode::String="Set")
@mapdef Entity "MaxHelpingHand/CustomDashRefillSpringDown" CustomDashRefillSpringDown(x::Integer, y::Integer, spriteDirectory::String="objects/MaxHelpingHand/twoDashRefillSpring",
    ignoreLighting::Bool=false, refillStamina::Bool=true, dashCount::Int=2, dashCountCap::Int=2, mode::String="Set")

const placements = Ahorn.PlacementDict(
    "Custom Dash Refill Spring (Up) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CustomDashRefillSpring
    ),
    "Custom Dash Refill Spring (Left) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CustomDashRefillSpringRight
    ),
    "Custom Dash Refill Spring (Right) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CustomDashRefillSpringLeft
    ),
    "Custom Dash Refill Spring (Down) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        CustomDashRefillSpringDown
    ),
)

function Ahorn.selection(entity::CustomDashRefillSpring)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y - 3, 12, 5)
end

function Ahorn.selection(entity::CustomDashRefillSpringLeft)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 1, y - 6, 5, 12)
end

function Ahorn.selection(entity::CustomDashRefillSpringRight)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 4, y - 6, 5, 12)
end

function Ahorn.selection(entity::CustomDashRefillSpringDown)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 6, y - 1, 12, 5)
end

const springsUnion = Union{CustomDashRefillSpring, CustomDashRefillSpringRight, CustomDashRefillSpringLeft, CustomDashRefillSpringDown}
Ahorn.editingOptions(entity::springsUnion) = Dict{String, Any}(
    "mode" => String["Set", "Add", "AddCapped"]
)

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomDashRefillSpring, room::Maple.Room) = Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "objects/MaxHelpingHand/twoDashRefillSpring") * "/00.png", 0, -8)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomDashRefillSpringLeft, room::Maple.Room) = Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "objects/MaxHelpingHand/twoDashRefillSpring") * "/00.png", 24, 0, rot=pi / 2)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomDashRefillSpringRight, room::Maple.Room) = Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "objects/MaxHelpingHand/twoDashRefillSpring") * "/00.png", -8, 16, rot=-pi / 2)
Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomDashRefillSpringDown, room::Maple.Room) = Ahorn.drawSprite(ctx, get(entity, "spriteDirectory", "objects/MaxHelpingHand/noDashRefillSpring") * "/00.png", 16, 24, rot=pi)

end
