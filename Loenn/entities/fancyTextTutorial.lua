local enums = require("consts.celeste_enums")

local birdNpc = {}

birdNpc.name = "MaxHelpingHand/FancyTextTutorial"
birdNpc.depth = -1000000
birdNpc.justification = {0.5, 1.0}
birdNpc.texture = "ahorn/MaxHelpingHand/greyscale_birb"
birdNpc.placements = {
    name = "bird",
    data = {
        birdId = "",
        dialogId = "TUTORIAL_DREAMJUMP",
        textScale = 1.0,
        direction = "Down",
        onlyOnce = false
    }
}
birdNpc.fieldInformation = {
    direction = {
        options = { "Up", "Down", "Left", "Right", "None" },
        editable = false
    }
}

return birdNpc
