local memo = {}

memo.name = "MaxHelpingHand/CustomCh3MemoOnFlagController"
memo.texture = "ahorn/MaxHelpingHand/set_flag_on_spawn"

memo.placements = {
    name = "memo",
    data = {
        paperSpriteFolderName = "",
        dialogId = "CH3_MEMO",
        dialogBeforeId = "CH3_MEMO_OPENING",
        dialogAfterId = "",
        flagOnCompletion = "",
        dialogBeforeOnlyOnce = false,
        dialogAfterOnlyOnce = false,
        flag = "flag",
        flagInverted = false,
        flagReusable = false,
        onlyOnce = false,
        textOffsetY = 210.0
    }
}

return memo
