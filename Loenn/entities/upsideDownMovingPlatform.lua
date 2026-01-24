local resortPlatformHelper = require("helpers.resort_platforms")
local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local textures = {
    "default", "cliffside"
}

local movingPlatform = {}

movingPlatform.name = "MaxHelpingHand/UpsideDownMovingPlatform"
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
            emitSound = true,
            pushPlayer = false,
            drawTracks = true,
            accurateTiming = true
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

function addPlatformSprites(sprites, entity, position, texture)
    local textureLocation = "objects/woodPlatform/%s"
    texture = texture or string.format(textureLocation, entity.texture or "default")
    width = entity.width or 16

    for i = 8, width - 1, 8 do
        local sprite = drawableSprite.fromTexture(texture, position)

        sprite:addPosition(i - 8, 0)
        sprite:useRelativeQuad(8, 0, 8, 8)
        sprite:setJustification(0, 0)
        sprite:addPosition(8, 8)
        sprite.rotation = math.pi

        table.insert(sprites, sprite)
    end

    local leftSprite = drawableSprite.fromTexture(texture, position)
    local rightSprite = drawableSprite.fromTexture(texture, position)
    local middleSprite = drawableSprite.fromTexture(texture, position)

    leftSprite:useRelativeQuad(25, 0, 8, 8)
    leftSprite:addPosition(8, 8)
    leftSprite:setJustification(0, 0)
    leftSprite.rotation = math.pi

    rightSprite:useRelativeQuad(0, 0, 8, 8)
    rightSprite:addPosition(width, 8)
    rightSprite:setJustification(0, 0)
    rightSprite.rotation = math.pi

    middleSprite:useRelativeQuad(16, 0, 8, 8)
    middleSprite:addPosition(math.floor(width / 2) + 4, 8)
    middleSprite:setJustification(0, 0)
    middleSprite.rotation = math.pi

    table.insert(sprites, leftSprite)
    table.insert(sprites, rightSprite)
    table.insert(sprites, middleSprite)

    return sprites
end

function movingPlatform.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local nodes = entity.nodes or {{x = 0, y = 0}}

    local drawTracks = entity.drawTracks or entity.drawTracks == nil

    local previousX, previousY = x, y
    for i, node in ipairs(nodes) do
        if drawTracks then
            resortPlatformHelper.addConnectorSprites(sprites, entity, previousX, previousY, node.x, node.y)
        end
        previousX, previousY = node.x, node.y
    end

    if (entity.mode == "Loop" or entity.mode == "LoopNoPause") and drawTracks then
        resortPlatformHelper.addConnectorSprites(sprites, entity, previousX, previousY, x, y)
    end

    addPlatformSprites(sprites, entity, entity)

    return sprites
end

function movingPlatform.nodeSprite(room, entity, node)
    return addPlatformSprites({}, entity, node)
end

movingPlatform.selection = resortPlatformHelper.getSelection

return movingPlatform
