local badelineSprite = {}

badelineSprite.name = "MaxHelpingHand/BadelineSprite"
badelineSprite.depth = 0
badelineSprite.justification = {0.5, 1.0}
badelineSprite.placements = {
    name = "badelineSprite",
    data = {
        left = false,
        floating = false
    }
}

function badelineSprite.scale(room, entity)
    return entity.left and -1 or 1, 1
end

function badelineSprite.texture(room, entity)
    return "characters/badeline/idle00"
end

return badelineSprite
