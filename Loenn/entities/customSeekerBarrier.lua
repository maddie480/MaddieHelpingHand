local seekerBarrier = {}

seekerBarrier.name = "MaxHelpingHand/CustomSeekerBarrier"
seekerBarrier.depth = 0
seekerBarrier.color = {0.25, 0.25, 0.25, 0.8}
seekerBarrier.placements = {
    name = "seeker_barrier",
    data = {
        width = 8,
        height = 8,
        color = "FFFFFF",
        particleColor = "FFFFFF",
        transparency = 0.15,
        particleTransparency = 0.5,
        particleDirection = 0.0,
        wavy = true
    }
}

seekerBarrier.fieldInformation = {
    color = {
        fieldType = "color"
    },
    particleColor = {
        fieldType = "color"
    }
}

return seekerBarrier
