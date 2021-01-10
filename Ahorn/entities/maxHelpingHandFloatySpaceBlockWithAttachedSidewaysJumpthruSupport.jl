module MaxHelpingHandFloatySpaceBlockWithAttachedSidewaysJumpthruSupport

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/FloatySpaceBlockWithAttachedSidewaysJumpthruSupport" FloatySpaceBlockWithAttachedSidewaysJumpthruSupport(x::Integer, y::Integer, 
    width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, tiletype::String="3", disableSpawnOffset::Bool=false)
    
const placements = Ahorn.PlacementDict(
    "Floaty Space Block (supporting Sideways Jumpthrus) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        FloatySpaceBlockWithAttachedSidewaysJumpthruSupport,
        "rectangle",
        Dict{String, Any}(),
        Ahorn.tileEntityFinalizer
    )
)

Ahorn.editingOptions(entity::FloatySpaceBlockWithAttachedSidewaysJumpthruSupport) = Dict{String, Any}(
    "tiletype" => Ahorn.tiletypeEditingOptions()
)

Ahorn.minimumSize(entity::FloatySpaceBlockWithAttachedSidewaysJumpthruSupport) = 8, 8
Ahorn.resizable(entity::FloatySpaceBlockWithAttachedSidewaysJumpthruSupport) = true, true

Ahorn.selection(entity::FloatySpaceBlockWithAttachedSidewaysJumpthruSupport) = Ahorn.getEntityRectangle(entity)

Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::FloatySpaceBlockWithAttachedSidewaysJumpthruSupport, room::Maple.Room) = Ahorn.drawTileEntity(ctx, room, entity)

end
