module MaxHelpingHandFlagExitBlock

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/FlagExitBlock" FlagExitBlock(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    tileType::String="3", flag::String="flag_exit_block", playSound::Bool=true)

const placements = Ahorn.PlacementDict(
    "Flag Exit Block (max480's Helping Hand)" => Ahorn.EntityPlacement(
        FlagExitBlock,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    ),
)

Ahorn.materialTileTypeKey(entity::FlagExitBlock) = "tileType"

Ahorn.editingOptions(entity::FlagExitBlock) = Dict{String, Any}(
    "tileType" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::FlagExitBlock) = 8, 8
Ahorn.resizable(entity::FlagExitBlock) = true, true

Ahorn.selection(entity::FlagExitBlock) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FlagExitBlock, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity, alpha=0.7)

end
