local drawableSprite = require("structs.drawable_sprite")

local breakingBall = {}

breakingBall.name = "MaxHelpingHand/SpinnerBreakingBallFrostHelper"
breakingBall.depth = 100
breakingBall.placements = {
    name = "default",
    data = {
        color = "FFFFFF",
        spritePath = "MaxHelpingHand/SpinnerBreakBallBlue",
        startFloating = false
    }
}
breakingBall.associatedMods = { "MaxHelpingHand", "FrostHelper" }

breakingBall.fieldInformation = {
    color = {
        fieldType = "color"
    },
    spritePath = {
        options = {
            "MaxHelpingHand/spinner_breaking_ball_placeholder",
            "MaxHelpingHand/SpinnerBreakBallBlue",
            "MaxHelpingHand/SpinnerBreakBallRed",
            "MaxHelpingHand/SpinnerBreakBallPurple",
            "MaxHelpingHand/SpinnerBreakBallRainbow"
        }
    }
}

function breakingBall.sprite(room, entity)
    local sprite = drawableSprite.fromTexture(entity.spritePath, { x = entity.x, y = entity.y })
    sprite.y -= 10
    return sprite
end

return breakingBall
