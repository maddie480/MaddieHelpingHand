module MaxHelpingHandSetFlagOnActionController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/Pico8FlagController" Pico8FlagCompletionController(x::Integer, y::Integer, flagOnComplete::String="flag_name")
@mapdef Entity "MaxHelpingHand/Pico8FlagController" Pico8FlagBerriesController(x::Integer, y::Integer, flagOnComplete::String="flag_name")

const placements = Ahorn.PlacementDict(
    "Set Flag On PICO-8 Completion Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        Pico8FlagCompletionController
    ),
    "Set Flag On PICO-8 Berry Collect Controller (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        Pico8FlagBerriesController
    )
)

const union = Union{Pico8FlagCompletionController, Pico8FlagBerriesController}

function Ahorn.selection(entity::union)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::union, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/pico_8_controller", 0, 0)

end
