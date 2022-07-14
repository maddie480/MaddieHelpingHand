module MaxHelpingHandSetFlagOnActionController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/SetFlagOnActionController" SetFlagOnActionController(x::Integer, y::Integer, action::String="Climb", flag::String="flag_name", inverted::Bool=false)

const placements = Ahorn.PlacementDict(
    "Set Flag On Action Controller (max480's Helping Hand)" => Ahorn.EntityPlacement(
        SetFlagOnActionController
    )
)

Ahorn.editingOptions(entity::SetFlagOnActionController) = Dict{String, Any}(
    "action" => String["OnGround", "InAir", "Climb", "Dash", "Swim", "HoldItem", "NoDashLeft", "FullDashes", "NoStaminaLeft", "LowStamina", "FullStamina"]
)

function Ahorn.selection(entity::SetFlagOnActionController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SetFlagOnActionController, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/set_flag_on_action", 0, 0)

end
