local drawableSpriteStruct = require("structs.drawable_sprite")

local rotateSpinner = {}

local speeds = {
    slow = "Slow",
    normal = "Normal",
    fast = "Fast",
}

rotateSpinner.name = "MaxHelpingHand/ReskinnableStarRotateSpinner"
rotateSpinner.nodeLimits = {1, 1}
rotateSpinner.nodeLineRenderType = "circle"
rotateSpinner.depth = -50
rotateSpinner.placements = {}

for i = 1, 2 do
    local clockwise = i == 1
    local languageName = clockwise and "clockwise" or "counter_clockwise"

    table.insert(rotateSpinner.placements, {
        name = languageName,
        data = {
            clockwise = clockwise,
            spriteFolder = "danger/MaxHelpingHand/starSpinner",
            particleColors = "EA64B7|3EE852,67DFEA|E85351,EA582C|33BDE8"
        }
    })
end

local function getSprite(room, entity, alpha)
    local sprites = {}

    local starfishTexture = entity.spriteFolder .. "/idle0_00"
    table.insert(sprites, drawableSpriteStruct.fromTexture(starfishTexture, entity))

    if alpha then
        for _, sprite in ipairs(sprites) do
            sprite:setColor({1.0, 1.0, 1.0, alpha})
        end
    end

    return sprites
end

function rotateSpinner.sprite(room, entity)
    return getSprite(room, entity)
end

function rotateSpinner.nodeSprite(room, entity, node)
    local entityCopy = table.shallowcopy(entity)

    entityCopy.x = node.x
    entityCopy.y = node.y

    return getSprite(room, entityCopy, 0.3)
end

return rotateSpinner
