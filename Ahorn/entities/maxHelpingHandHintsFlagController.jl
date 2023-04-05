module MaxHelpingHandHintsFlagController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/HintsFlagController" HintsFlagController(x::Integer, y::Integer, outputFlag::String="hints", not::Bool=false)

const placements = Ahorn.PlacementDict(
    "Hints Flag Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        HintsFlagController
    )
)

function Ahorn.selection(entity::HintsFlagController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

function Ahorn.editingOptions(effect::HintsFlagController)
    return Dict{String, Any}(
    )
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HintsFlagController, room::Maple.Room)
    if entity.not
        Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/hints_flag_controller_inv", 0, 0)
    else
        Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/hints_flag_controller", 0, 0)
    end
end

end
