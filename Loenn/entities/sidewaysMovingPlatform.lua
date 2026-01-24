local drawableSpriteStruct = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local resortPlatformHelper = require("helpers.resort_platforms")

local textures = {
    "default", "cliffside"
}

local movingPlatform = {}

movingPlatform.name = "MaxHelpingHand/SidewaysMovingPlatform"
movingPlatform.depth = -9000
movingPlatform.canResize = {false, true}
movingPlatform.placements = {}
movingPlatform.nodeLimits = {1, -1}
movingPlatform.nodeLineRenderType = "line"

for i, texture in ipairs(textures) do
    movingPlatform.placements[i] = {
        name = texture,
        data = {
            height = 8,
            left = true,
            mode = "Loop",
            texture = texture,
            moveTime = 2.0,
            pauseTime = 0.0,
            easing = true,
            amount = 1,
            offset = 0.0,
            flag = "",
            emitSound = true,
            drawTracks = true,
            accurateTiming = true
        },
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

local function renderPlatform(room, entity, node)
    local texture = "objects/woodPlatform/" .. entity.texture

    local x, y = node.x or 0, node.y or 0
    local height = entity.height or 8

    local startX, startY = math.floor(x / 8) + 1, math.floor(y / 8) + 1
    local stopY = startY + math.floor(height / 8) - 1
    local len = stopY - startY

    local sprites = {}

    for i = 0, len do
        local quadX = 8

        if i == 0 then
            quadX = entity.left and 24 or 0

        elseif i == len then
            quadX = entity.left and 0 or 24
        end

        local sprite = drawableSpriteStruct.fromTexture(texture, node)

        sprite:setJustification(0, 0)
        sprite:addPosition(entity.left and 0 or 8, i * 8 + (entity.left and 8 or 0))
        sprite:useRelativeQuad(quadX, 0, 8, 8)
        sprite.rotation = entity.left and - math.pi / 2 or math.pi / 2

        table.insert(sprites, sprite)
    end

    local sprite = drawableSpriteStruct.fromTexture(texture, node)
    sprite:setJustification(0, 0)
    sprite:addPosition(entity.left and 0 or 8, len * 4 + (entity.left and 8 or 0))
    sprite:useRelativeQuad(16, 0, 8, 8)
    sprite.rotation = entity.left and - math.pi / 2 or math.pi / 2
    table.insert(sprites, sprite)

    return sprites
end

function movingPlatform.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {{x = 0, y = 0}}
    local offsetX = -4
    local offsetY = (entity.height / 2) - 4

    local drawTracks = entity.drawTracks or entity.drawTracks == nil

    local previousX, previousY = x, y
    for i, node in ipairs(nodes) do
        if drawTracks then
            resortPlatformHelper.addConnectorSprites(sprites, entity, previousX + offsetX, previousY + offsetY, node.x + offsetX, node.y + offsetY)
        end
        previousX, previousY = node.x, node.y
    end

    if (entity.mode == "Loop" or entity.mode == "LoopNoPause") and drawTracks then
        resortPlatformHelper.addConnectorSprites(sprites, entity, previousX + offsetX, previousY + offsetY, x + offsetX, y + offsetY)
    end

    local extraSprites = renderPlatform(room, entity, entity)
    for i, sprite in ipairs(extraSprites) do
        table.insert(sprites, sprite)
    end
    return sprites
end

function movingPlatform.nodeSprite(room, entity, node)
    return renderPlatform(room, entity, node)
end

function movingPlatform.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local height = entity.height or 16
    local nodes = entity.nodes or {}

    local mainRectangle = utils.rectangle(x, y, 8, height)
    local nodeRectangles = {}

    for i, node in ipairs(nodes) do
        nodeRectangles[i] = utils.rectangle(node.x, node.y, 8, height)
    end

    return mainRectangle, nodeRectangles
end

return movingPlatform
