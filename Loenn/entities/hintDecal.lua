local drawableSprite = require("structs.drawable_sprite")
local entities = require("entities")

local decal = {}

decal.name = "MaxHelpingHand/HintDecal"

decal.placements = {
    name = "decal",
    data = {
        texture = "1-forsakencity/sign_you_can_go_up",
        scaleX = 1.0,
        scaleY = 1.0,
        foreground = false
    }
}

function decal.depth(room, entity)
    return entity.foreground and -10500 or 9000
end

function decal.sprite(room, entity)
    local sprite = drawableSprite.fromTexture("decals/" .. entity.texture, entity)
    sprite:setScale(entity.scaleX, entity.scaleY)
    return sprite
end

-- Add a Custom Bird Tutorial preset for the "show hints" button
local customBirdTutorial = entities.registeredEntities["everest/customBirdTutorial"]

if customBirdTutorial.placements.name then
    -- turn "placements" into a table to be able to add another
    customBirdTutorial.placements = { customBirdTutorial.placements }
end

table.insert(customBirdTutorial.placements, {
    name = "maxhelpinghand_showhints",
    data = {
        faceLeft = true,
        birdId = "",
        onlyOnce = false,
        caw = true,
        info = "MAXHELPINGHAND_SHOWHINTS",
        controls = "ShowHints"
    }
})

return decal