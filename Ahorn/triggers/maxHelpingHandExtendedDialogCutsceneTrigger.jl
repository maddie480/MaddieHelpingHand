module MaxHelpingHandExtendedDialogCutsceneTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/ExtendedDialogCutsceneTrigger" ExtendedDialogCutsceneTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    endLevel::Bool=false, onlyOnce::Bool=true, dialogId::String="", deathCount::Int=-1, font::String="")

const placements = Ahorn.PlacementDict(
    "Dialog Cutscene (Extended) (max480's Helping Hand)" => Ahorn.EntityPlacement(
        ExtendedDialogCutsceneTrigger,
        "rectangle"
    )
)

end