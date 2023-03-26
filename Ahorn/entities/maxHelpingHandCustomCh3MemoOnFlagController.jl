module MaxHelpingHandCustomCh3MemoOnFlagController

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomCh3MemoOnFlagController" CustomCh3MemoOnFlagController(x::Integer, y::Integer, paperSpriteFolderName::String="", dialogId::String="CH3_MEMO",
    dialogBeforeId::String="CH3_MEMO_OPENING", dialogAfterId::String="", flagOnCompletion::String="", dialogBeforeOnlyOnce::Bool=false, dialogAfterOnlyOnce::Bool=false,
    flag::String="flag", flagInverted::Bool=false, flagReusable::Bool=false, onlyOnce::Bool=false, nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
    "Custom Chapter 3 Memo On Flag Controller\n(max480's Helping Hand)" => Ahorn.EntityPlacement(
        CustomCh3MemoOnFlagController,
        "rectangle"
    )
)

function Ahorn.selection(entity::CustomCh3MemoOnFlagController)
    x, y = Ahorn.position(entity)

    return Ahorn.Rectangle(x - 12, y - 12, 24, 24)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomCh3MemoOnFlagController, room::Maple.Room) = Ahorn.drawSprite(ctx, "ahorn/MaxHelpingHand/set_flag_on_spawn", 0, 0)

end
