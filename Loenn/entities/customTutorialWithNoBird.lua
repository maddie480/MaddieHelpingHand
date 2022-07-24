local enums = require("consts.celeste_enums")

local birdNpc = {}

birdNpc.name = "MaxHelpingHand/CustomTutorialWithNoBird"
birdNpc.depth = -1000000
birdNpc.justification = {0.5, 1.0}
birdNpc.texture = "ahorn/MaxHelpingHand/greyscale_birb"
birdNpc.placements = {
    name = "bird",
    data = {
        birdId = "",
        onlyOnce = false,
        info = "TUTORIAL_DREAMJUMP",
        controls = "DownRight,+,Dash,tinyarrow,Jump",
        direction = "Down"
    }
}
birdNpc.fieldInformation = {
    info = {
        options = enums.everest_bird_tutorial_tutorials
    },
    direction = {
        options = { "Up", "Down", "Left", "Right", "None" },
        editable = false
    }
}

return birdNpc
