module MaxHelpingHandStrawberryCollectionField

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/StrawberryCollectionField" StrawberryCollectionField(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    delayBetweenBerries::Bool=true, includeGoldens::Bool=false)

const placements = Ahorn.PlacementDict(
    "Strawberry Collection Field (max480's Helping Hand)" => Ahorn.EntityPlacement(
        StrawberryCollectionField,
        "rectangle"
    )
)

end
