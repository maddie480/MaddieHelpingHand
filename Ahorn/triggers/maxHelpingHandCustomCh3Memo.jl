module MaxHelpingHandCustomCh3Memo

using ..Ahorn, Maple

@mapdef Trigger "MaxHelpingHand/CustomCh3Memo" CustomCh3Memo(x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    paperSpriteFolderName::String="", dialogId::String="CH3_MEMO", dialogBeforeId::String="CH3_MEMO_OPENING", dialogAfterId::String="", flagOnCompletion::String="",
    nodes::Array{Tuple{Integer, Integer}, 1}=Tuple{Integer, Integer}[])

const placements = Ahorn.PlacementDict(
    "Custom Chapter 3 Memo (max480's Helping Hand)" => Ahorn.EntityPlacement(
        CustomCh3Memo,
        "rectangle"
    )
)

function Ahorn.nodeLimits(trigger::CustomCh3Memo)
    return 0, 1
end

end
