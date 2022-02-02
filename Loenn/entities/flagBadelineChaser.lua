local badelineChaser = {}

badelineChaser.name = "MaxHelpingHand/FlagBadelineChaser"
badelineChaser.depth = 0
badelineChaser.justification = {0.5, 1.0}
badelineChaser.texture = "characters/badeline/sleep00"
badelineChaser.placements = {
    name = "dark_chaser",
    data = {
        canChangeMusic = true,
        flag = "flag_name",
        index = 0,
    }
}

badelineChaser.fieldInformation = {
    index = {
        fieldType = "integer"
    }
}

return badelineChaser
