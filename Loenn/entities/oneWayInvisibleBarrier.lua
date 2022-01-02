local invisibleBarrier = {}

invisibleBarrier.name = "MaxHelpingHand/OneWayInvisibleBarrierHorizontal"
invisibleBarrier.fillColor = {0.4, 0.4, 0.4, 0.8}
invisibleBarrier.borderColor = {0.0, 0.0, 0.0, 0.0}
invisibleBarrier.canResize = {false, true}
invisibleBarrier.placements = {
    {
        name = "left",
        data = {
            width = 8,
            height = 8,
            left = true,
            letSeekersThrough = false
        }
    },
    {
        name = "right",
        data = {
            width = 8,
            height = 8,
            left = false,
            letSeekersThrough = false
        }
    }
}

return invisibleBarrier
