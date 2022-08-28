local drawableSprite = require("structs.drawable_sprite")

local flagDecal = {}

flagDecal.name = "MaxHelpingHand/FlagDecal"

function flagDecal.depth(room, entity)
    return entity.depth or 0
end

flagDecal.placements = {
    name = "flag_decal",
    data = {
        fps = 12.0,
        flag = "decal_flag",
        inverted = false,
        decalPath = "1-forsakencity/flag",
        appearAnimationPath = "",
        disappearAnimationPath = "",
        depth = 8999
    }
}

flagDecal.fieldInformation = {
    depth = {
        fieldType = "integer",
        options = {
            ["In front of FG"] = -10501,
            ["Behind FG"] = -10499,
            ["In front of BG"] = 8999,
            ["Behind BG"] = 9001,
        }
    }
}

function flagDecal.texture(room, entity)
    if drawableSprite.fromTexture("decals/" .. entity.decalPath .. "00") ~= nil then
        return "decals/" .. entity.decalPath .. "00"
    else
        return "decals/" .. entity.decalPath
    end
end

return flagDecal
