local drawableRectangle = require("structs.drawable_rectangle")
local utils = require("utils")

local flagDecal = {}

flagDecal.name = "MaxHelpingHand/FlagDecalXML"

function flagDecal.depth(room, entity)
    return entity.depth or 0
end

flagDecal.placements = {
    name = "flag_decal",
    data = {
        flag = "decal_flag",
        inverted = false,
        sprite = "",
        depth = 8999,
        scaleX = 1.0,
        scaleY = 1.0,
        rotation = 0.0
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

flagDecal.texture = "ahorn/MaxHelpingHand/flag_decal_xml"

function flagDecal.scale(room, entity)
    return { entity.scaleX or 1, entity.scaleY or 1 }
end
function flagDecal.rotation(room, entity)
    return entity.rotation * math.pi / 180
end

return flagDecal
