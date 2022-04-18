local player = {}

player.name = "MaxHelpingHand/MadelineSprite"
player.depth = 0
player.justification = {0.5, 1.0}
player.placements = {
    name = "player",
    data = {
        hasBackpack = true,
        left = false,
        dashCount = 1
    }
}

player.fieldInformation = {
    dashCount = {
        fieldType = "integer"
    }
}

function player.scale(room, entity)
    return entity.left and -1 or 1, 1
end

function player.texture(room, entity)
    if entity.hasBackpack then
        return "characters/player/sitDown00"
    else
        return "characters/player_no_backpack/sitDown00"
    end
end

return player
