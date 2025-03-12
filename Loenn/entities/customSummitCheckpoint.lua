local drawableSprite = require("structs.drawable_sprite")

local summitCheckpoint = {}

summitCheckpoint.name = "MaxHelpingHand/CustomSummitCheckpoint"
summitCheckpoint.depth = 8999

summitCheckpoint.placements = {
    name = "summit_checkpoint",
    data = {
        firstDigit = "zero",
        secondDigit = "zero",
        spriteDirectory = "MaxHelpingHand/summitcheckpoints",
        confettiColors = "fe2074,205efe,cefe20",
        groupFlag = ""
    }
}

local numberlist =  { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "minus", "questionmark" }

summitCheckpoint.fieldInformation = {
    firstDigit = {
        options = numberlist
    },
    secondDigit = {
        options = numberlist
    },
    confettiColors = {
        fieldType = "list",
        minimumElements = 1,
        elementDefault = "ffffff",
        elementOptions = {
            fieldType = "color",
            allowXNAColors = false
        }
    }
}

local backTexture = "%s/base02"
local digitBackground = "%s/%s/numberbg"
local digitForeground = "%s/%s/number"

function summitCheckpoint.sprite(room, entity)
    local directory = entity.spriteDirectory
    local digit1 = entity.firstDigit
    local digit2 = entity.secondDigit

    local backSprite = drawableSprite.fromTexture(string.format(backTexture, directory), entity)
    local backDigit1 = drawableSprite.fromTexture(string.format(digitBackground, directory, digit1), entity)
    local frontDigit1 = drawableSprite.fromTexture(string.format(digitForeground, directory, digit1), entity)
    local backDigit2 = drawableSprite.fromTexture(string.format(digitBackground, directory, digit2), entity)
    local frontDigit2 = drawableSprite.fromTexture(string.format(digitForeground, directory, digit2), entity)

    backDigit1:addPosition(-2, 4)
    frontDigit1:addPosition(-2, 4)
    backDigit2:addPosition(2, 4)
    frontDigit2:addPosition(2, 4)

    local sprites = {
        backSprite,
        backDigit1,
        backDigit2,
        frontDigit1,
        frontDigit2
    }

    return sprites
end

return summitCheckpoint
