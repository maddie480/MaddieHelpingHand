local trigger = {}

trigger.name = "MaxHelpingHand/CustomCh3Memo"
trigger.nodeLimits = {0, 1}

trigger.placements = {
    name = "trigger",
    data = {
        paperSpriteFolderName = "",
        dialogId = "CH3_MEMO",
        dialogBeforeId = "CH3_MEMO_OPENING",
        dialogAfterId = "",
        flagOnCompletion = "",
        dialogBeforeOnlyOnce = false,
        dialogAfterOnlyOnce = false
    }
}

return trigger
