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

flagDecal.texture = "ahorn/MaxHelpingHand/flag_decal_xml"

return flagDecal
