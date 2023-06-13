local utils = require("utils")
local drawableSprite = require("structs.drawable_sprite")

local breakerBox = {}

breakerBox.name = "MaxHelpingHand/FlagBreakerBox"
breakerBox.depth = -10550
breakerBox.placements = {
  name = "breaker_box",
  data = {
    flag = "",
    health = 2,
    floaty = true,
    bouncy = true,
    sprite = "breakerBox",
    flipX = false,
    music = "",
    music_progress = -1,
    music_session = false,
    surfaceIndex = 9,
    color = "fffc75",
    color2 = "6bffff",
    refill = true
  }
}
breakerBox.fieldInformation = {
  health = {
    fieldType = "integer",
    minimumValue = 1
  },
  music_progress = {
    fieldType = "integer",
    minimumValue = -1
  },
  surfaceIndex = {
    fieldType = "integer"
  },
  color = {
    fieldType = "color"
  },
  color2 = {
    fieldType = "color"
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
