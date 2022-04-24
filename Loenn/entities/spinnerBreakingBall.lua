local drawableSprite = require("structs.drawable_sprite")

local breakingBall = {}

breakingBall.name = "MaxHelpingHand/SpinnerBreakingBall"
breakingBall.depth = 100
breakingBall.placements = {
    {
        name = "blue",
        data = {
            color = "Blue",
            spritePath = "MaxHelpingHand/spinner_breaking_ball_placeholder",
            startFloating = false
        }
    },
    {
        name = "red",
        data = {
            color = "Red",
            spritePath = "MaxHelpingHand/spinner_breaking_ball_placeholder",
            startFloating = false
        }
    },
    {
        name = "purple",
        data = {
            color = "Purple",
            spritePath = "MaxHelpingHand/spinner_breaking_ball_placeholder",
            startFloating = false
        }
    },
    {
        name = "rainbow",
        data = {
            color = "Rainbow",
            spritePath = "MaxHelpingHand/spinner_breaking_ball_placeholder",
            startFloating = false
        }
    }
}

breakingBall.fieldInformation = {
    color = {
        options = { "Blue", "Red", "Purple", "Rainbow" },
        editable = false
    }
}

function breakingBall.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(entity.spritePath, { x = entity.x, y = entity.y })
    sprite.y -= 10
    return sprite
end

return breakingBall
