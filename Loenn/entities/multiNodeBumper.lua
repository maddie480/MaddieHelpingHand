local bumper = {}

bumper.name = "MaxHelpingHand/MultiNodeBumper"
bumper.depth = 0
bumper.nodeLineRenderType = "line"
bumper.texture = "objects/Bumper/Idle22"
bumper.nodeLimits = {1, -1}
bumper.placements = {
    name = "bumper",
    data = {
        mode = "Loop",
        moveTime = 2.0,
        pauseTime = 0.0,
        easing = true,
        notCoreMode = false,
        amount = 1,
        offset = 0.0,
        wobble = false,
        flag = ""
    }
}

bumper.fieldInformation = {
    amount = {
        fieldType = "integer"
    }
}

return bumper
