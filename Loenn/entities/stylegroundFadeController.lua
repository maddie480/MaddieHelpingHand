local sgController = {}

sgController.name = "MaxHelpingHand/StylegroundFadeController"
sgController.depth = 0
sgController.texture = "@Internal@/northern_lights"
sgController.placements = {
    name = "controller",
    data = {
        flag = "flag1,flag2",
        fadeInTime = 1.0,
        fadeOutTime = 1.0,
        notFlag = false
    }
}

return sgController
