local strawberry = {}

strawberry.name = "MaxHelpingHand/GoldenStrawberryCustomConditions"
strawberry.depth = -100

strawberry.texture = "collectables/goldberry/idle00"

strawberry.placements = {
    {
        name = "golden",
        data = {
            mustNotDieAndVisitFurtherRooms = true,
            mustHaveUnlockedCSides = true,
            mustHaveCompletedLevel = true,
            showGoldenChapterCard = true
        },
    },
}

return strawberry
