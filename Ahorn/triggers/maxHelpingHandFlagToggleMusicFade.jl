module MaxHelpingHandFlagToggleMusicFadeTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/FlagToggleMusicFadeTrigger" FlagToggleMusicFadeTrigger(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    direction::String="leftToRight", fadeA::Number=0.0, fadeB::Number=1.0, parameter::String="", flag::String="flag_toggle_music_fade", inverted::Bool=false)

const placements = Ahorn.PlacementDict(
    "Music Fade (Flag-Toggled) (Maddie's Helping Hand)" => Ahorn.EntityPlacement(
        FlagToggleMusicFadeTrigger,
        "rectangle"
    )
)

function Ahorn.editingOptions(trigger::FlagToggleMusicFadeTrigger)
    return Dict{String, Any}(
        "direction" => Maple.music_fade_trigger_directions
    )
end

end
