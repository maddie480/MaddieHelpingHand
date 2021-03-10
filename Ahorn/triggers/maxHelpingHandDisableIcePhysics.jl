module MaxHelpingHandDisableIcePhysicsTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/DisableIcePhysicsTrigger" DisableIcePhysicsTrigger(x::Integer, y::Integer,
    width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight, disableIcePhysics::Bool=true)

const placements = Ahorn.PlacementDict(
    "Disable Ice Physics (max480's Helping Hand)" => Ahorn.EntityPlacement(
        DisableIcePhysicsTrigger,
        "rectangle"
    )
)

end
