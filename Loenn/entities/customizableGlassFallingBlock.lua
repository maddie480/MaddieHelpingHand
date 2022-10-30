local glassBlock = {}

glassBlock.name = "MaxHelpingHand/CustomizableGlassFallingBlock"
glassBlock.fillColor = {1.0, 1.0, 1.0, 0.6}
glassBlock.borderColor = {1.0, 1.0, 1.0, 0.8}
glassBlock.placements = {
    name = "glass_block",
    data = {
        behindFgTiles = false,
        width = 8,
        height = 8,
        climbFall = true
    }
}

glassBlock.depth = 0

return glassBlock
