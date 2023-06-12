local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local breakerBox = {}

breakerBox.name = "MaxHelpingHand/FlagBreakerBox"
breakerBox.depth = -10550
breakerBox.fieldInformation = {
  health = {
    fieldType = "integer"
  },
  music_progress = {
    fieldType = "integer"
  },
  surfaceIndex = {
    fieldType = "integer"
  },
  smashColor = {
    fieldType = "color"
  },
  sparkColor = {
    fieldType = "color"
  }
}
breakerBox.placements = {
  name = "breaker_box",
  data = {
    flag = "",
    health = 2,
    sprite = "breakerBox",
    flipX = false,
    music = "",
    music_progress = -1,
    music_session = false,
    surfaceIndex = 9,
    smashColor = "FFFC75",
    sparkColor = "FFFC75"
  }
}

function breakerBox.texture(room, entity)
  return "objects/" .. entity.sprite .. "/Idle00"
end

function breakerBox.scale(room, entity)
  local scaleX = entity.flipX and -1 or 1

  return scaleX, 1
end

function breakerBox.justification(room, entity)
  local flipX = entity.flipX

  return flipX and 0.75 or 0.25, 0.25
end

return breakerBox
