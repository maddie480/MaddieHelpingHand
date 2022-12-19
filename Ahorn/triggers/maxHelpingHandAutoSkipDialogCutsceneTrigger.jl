module MaxHelpingHandAutoSkipDialogCutsceneTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/AutoSkipDialogCutsceneTrigger" AutoSkipDialogCutsceneTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    endLevel::Bool=false, onlyOnce::Bool=true, dialogId::String="", deathCount::Int=-1)

const placements = Ahorn.PlacementDict(
    "Auto-Skip Dialog Cutscene (max480's Helping Hand)" => Ahorn.EntityPlacement(
        AutoSkipDialogCutsceneTrigger,
        "rectangle"
    )
)

end