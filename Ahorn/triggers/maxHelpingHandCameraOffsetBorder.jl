module MaxHelpingHandCameraOffsetBorder

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/CameraOffsetBorder" CameraOffsetBorder(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    topLeft::Bool=true, topCenter::Bool=true, topRight::Bool=true, centerLeft::Bool=true, centerRight::Bool=true, bottomLeft::Bool=true, bottomCenter::Bool=true, bottomRight::Bool=true,
    flag::String="", inside::Bool=false, inverted::Bool=false)

const placements = Ahorn.PlacementDict(
    "Camera Offset Border (max480's Helping Hand)" => Ahorn.EntityPlacement(
        CameraOffsetBorder,
        "rectangle"
    )
)

Ahorn.editingOrder(entity::CameraOffsetBorder) = String["x", "y", "width", "height", "topLeft", "topCenter", "topRight", "centerLeft", "centerRight", "bottomLeft", "bottomCenter", "bottomRight", "flag", "inverted", "inside"]

end
