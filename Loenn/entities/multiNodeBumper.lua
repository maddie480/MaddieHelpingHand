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
        flag = "",
        accurateTiming = true
    }
}

bumper.fieldInformation = {
    amount = {
        fieldType = "integer"
    },
    mode = {
        options = { "Loop", "LoopNoPause", "BackAndForth", "BackAndForthNoPause", "TeleportBack" },
        editable = false
    }
}

return bumper
