module MaxHelpingHandFlagLogicGate

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/FlagLogicGate" FlagLogicGate(x::Integer, y::Integer, inputFlags::String="flag1,!flag2,flag3", outputFlag::String="flag4", func::String="AND", not::Bool=false)

const placements = Ahorn.PlacementDict(
    "Flag Logic Gate (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        FlagLogicGate
    )
)

function Ahorn.selection(entity::FlagLogicGate)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

function Ahorn.editingOptions(effect::FlagLogicGate)
    return Dict{String, Any}(
        "func" => String["AND", "OR", "XOR"]
    )
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlagLogicGate, room::Maple.Room)
    text = entity.func
    if entity.not
        if entity.func == "XOR"
            # this one is known as XNOR in electronics, not NXOR, so...
            text = "XNOR"
        else
            text = "N" * entity.func
        end
    end

    Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/flag_logic_gate", 0, 0)
    Ahorn.drawCenteredText(ctx, text, -12, 1, 24, 8, tint=(0.0, 0.0, 0.0, 1.0))
end

end
