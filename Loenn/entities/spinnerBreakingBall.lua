local drawableSprite = require("structs.drawable_sprite")

local breakingBall = {}

breakingBall.name = "MaxHelpingHand/SpinnerBreakingBall"
breakingBall.depth = 100
breakingBall.placements = {
    {
        name = "blue",
        data = {
            color = "Blue",
            spritePath = "MaxHelpingHand/SpinnerBreakBallBlue",
            startFloating = false,
            rainbowTinting = false
        }
    },
    {
        name = "red",
        data = {
            color = "Red",
            spritePath = "MaxHelpingHand/SpinnerBreakBallRed",
            startFloating = false,
            rainbowTinting = false
        }
    },
    {
        name = "purple",
        data = {
            color = "Purple",
            spritePath = "MaxHelpingHand/SpinnerBreakBallPurple",
            startFloating = false,
            rainbowTinting = false
        }
    },
    {
        name = "rainbow",
        data = {
            color = "Rainbow",
            spritePath = "MaxHelpingHand/SpinnerBreakBallRainbow",
            startFloating = false,
            rainbowTinting = true
        }
    }
}

breakingBall.fieldInformation = {
    color = {
        options = { "Blue", "Red", "Purple", "Rainbow" },
        editable = false
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
