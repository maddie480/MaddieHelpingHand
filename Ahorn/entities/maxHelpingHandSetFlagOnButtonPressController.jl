module MaxHelpingHandSetFlagOnButtonPressController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/SetFlagOnButtonPressController" SetFlagOnButtonPressController(x::Integer, y::Integer, button::String="Grab", flag::String="flag_name", inverted::Bool=false, toggleMode::Bool=false)

const placements = Ahorn.PlacementDict(
    "Set Flag On Button Press Controller (max480's Helping Hand)" => Ahorn.EntityPlacement(
        SetFlagOnButtonPressController
    )
)

Ahorn.editingOptions(entity::SetFlagOnButtonPressController) = Dict{String, Any}(
    "button" => String[
        "Jump",
        "Dash",
        "Grab",
        "Talk",
        "CrouchDash",
        "ESC",
        "Pause",
        "MenuLeft",
        "MenuRight",
        "MenuUp",
        "MenuDown",
        "MenuConfirm",
        "MenuCancel",
        "MenuJournal",
        "QuickRestart"
    ]
)

function Ahorn.selection(entity::SetFlagOnButtonPressController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SetFlagOnButtonPressController, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/set_flag_on_button", 0, 0)

end
