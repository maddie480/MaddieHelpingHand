local resortPlatformHelper = require("helpers.resort_platforms")
local utils = require("utils")

local textures = {
    "default", "cliffside"
}

local movingPlatform = {}

movingPlatform.name = "MaxHelpingHand/MultiNodeMovingPlatform"
movingPlatform.depth = 1
movingPlatform.nodeLimits = {1, -1}
movingPlatform.nodeLineRenderType = "line"
movingPlatform.placements = {}

for i, texture in ipairs(textures) do
    movingPlatform.placements[i] = {
        name = texture,
        data = {
            width = 8,
            mode = "Loop",
            texture = texture,
            moveTime = 2.0,
            pauseTime = 0.0,
            easing = true,
            amount = 1,
            offset = 0.0,
            flag = "",
            moveLater = true,
            emitSound = true
        }
    }
end

movingPlatform.fieldInformation = {
    amount = {
        fieldType = "integer"
    },
    mode = {
        options = { "Loop", "LoopNoPause", "BackAndForth", "BackAndForthNoPause", "TeleportBack" },
        editable = false
    },
    texture = {
        options = textures
    }
}

function movingPlatform.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {{x = 0, y = 0}}

    local previousX, previousY = x, y
    for i, node in ipairs(nodes) do
        resortPlatformHelper.addConnectorSprites(sprites, entity, previousX, previousY, node.x, node.y)
        previousX, previousY = node.x, node.y
    end

    if entity.mode == "Loop" or entity.mode == "LoopNoPause" then
        resortPlatformHelper.addConnectorSprites(sprites, entity, previousX, previousY, x, y)
    end

    resortPlatformHelper.addPlatformSprites(sprites, entity, entity)

    return sprites
end

function movingPlatform.nodeSprite(room, entity, node)
    return resortPlatformHelper.addPlatformSprites({}, entity, node)
end

movingPlatform.selection = resortPlatformHelper.getSelection

return movingPlatform
