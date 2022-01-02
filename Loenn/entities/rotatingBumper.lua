local drawableSpriteStruct = require("structs.drawable_sprite")

local rotatingBumper = {}

rotatingBumper.name = "MaxHelpingHand/RotatingBumper"
rotatingBumper.nodeLimits = {1, 1}
rotatingBumper.nodeLineRenderType = "circle"
rotatingBumper.depth = 0
rotatingBumper.placements = {}

rotatingBumper.placements = {
    name = "bumper",
    data = {
        speed = 360.0,
        attachToCenter = false,
        notCoreMode = false,
        wobble = false
    }
}

local function getSprite(room, entity, alpha)
    local sprites = {}

    local bumperTexture = "objects/Bumper/Idle22"
    table.insert(sprites, drawableSpriteStruct.fromTexture(bumperTexture, entity))

    if alpha then
        for _, sprite in ipairs(sprites) do
            sprite:setColor({1.0, 1.0, 1.0, alpha})
        end
    end

    return sprites
end

function rotatingBumper.sprite(room, entity)
    return getSprite(room, entity)
end

function rotatingBumper.nodeSprite(room, entity, node)
    local entityCopy = table.shallowcopy(entity)

    entityCopy.x = node.x
    entityCopy.y = node.y

    return getSprite(room, entityCopy, 0.3)
end

return rotatingBumper
