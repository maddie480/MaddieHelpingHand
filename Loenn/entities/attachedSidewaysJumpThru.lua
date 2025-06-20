local drawableSpriteStruct = require("structs.drawable_sprite")
local drawing = require("utils.drawing")
local utils = require("utils")
local enums = require("consts.celeste_enums")

local function getTexture(entity)
    return entity.texture and entity.texture ~= "default" and entity.texture or "wood"
end

local jumpthru = {}

local textures = {"wood", "dream", "temple", "templeB", "cliffside", "reflection", "core", "moon"}

jumpthru.name = "MaxHelpingHand/AttachedSidewaysJumpThru"
jumpthru.depth = -9000
jumpthru.canResize = {false, true}
jumpthru.fieldInformation = {
    texture = {
        options = textures
    },
    surfaceIndex = {
        options = enums.tileset_sound_ids,
        fieldType = "integer"
    }
}
jumpthru.placements = {}

jumpthru.placements = {
    {
        name = "left",
        data = {
            height = 8,
            left = true,
            texture = "wood",
            animationDelay = 0.0,
            letSeekersThrough = false,
            surfaceIndex = -1,
            pushPlayer = false,
			cornerCorrect = false
        },
    },
    {
        name = "right",
        data = {
            height = 8,
            left = false,
            texture = "wood",
            animationDelay = 0.0,
            letSeekersThrough = false,
            surfaceIndex = -1,
            pushPlayer = false,
			cornerCorrect = false
        },
    }
}

function jumpthru.sprite(room, entity)
    local textureRaw = getTexture(entity)
    local texture = "objects/jumpthru/" .. textureRaw

    local x, y = entity.x or 0, entity.y or 0
    local height = entity.height or 8

    local startX, startY = math.floor(x / 8) + 1, math.floor(y / 8) + 1
    local stopY = startY + math.floor(height / 8) - 1
    local len = stopY - startY

    local sprites = {}

    for i = 0, len do
        local quadX = 8
        local quadY = 8

        if i == 0 then
            quadX = entity.left and 16 or 0
            quadY = room.tilesFg.matrix:get(startX, startY - 1, "0") ~= "0" and 0 or 8

        elseif i == len then
            quadX = entity.left and 0 or 16
            quadY = room.tilesFg.matrix:get(startX, stopY + 1, "0") ~= "0" and 0 or 8
        end

        local sprite = drawableSpriteStruct.fromTexture(texture, entity)

        sprite:setJustification(0, 0)
        sprite:addPosition(entity.left and 0 or 8, i * 8 + (entity.left and 8 or 0))
        sprite:useRelativeQuad(quadX, quadY, 8, 8)
        sprite.rotation = entity.left and - math.pi / 2 or math.pi / 2

        table.insert(sprites, sprite)
    end

    return sprites
end

function jumpthru.selection(room, entity)
    return utils.rectangle(entity.x, entity.y, 8, entity.height)
end

return jumpthru
