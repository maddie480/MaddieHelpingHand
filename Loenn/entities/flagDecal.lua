local drawableSprite = require("structs.drawable_sprite")

local flagDecal = {}

flagDecal.name = "MaxHelpingHand/FlagDecal"
flagDecal.depth = 0
flagDecal.placements = {
    name = "flag_decal",
    data = {
        fps = 12.0,
        flag = "decal_flag",
        inverted = false,
        decalPath = "1-forsakencity/flag",
        appearAnimationPath = "",
        disappearAnimationPath = ""
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
