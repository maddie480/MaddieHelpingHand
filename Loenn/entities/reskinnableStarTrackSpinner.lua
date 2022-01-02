local drawableSpriteStruct = require("structs.drawable_sprite")

local trackSpinner = {}

local speeds = {
    slow = "Slow",
    normal = "Normal",
    fast = "Fast",
}

trackSpinner.name = "MaxHelpingHand/ReskinnableStarTrackSpinner"
trackSpinner.nodeLimits = {1, 1}
trackSpinner.nodeLineRenderType = "line"
trackSpinner.depth = -50
trackSpinner.placements = {}

for speedName, speedValue in pairs(speeds) do
    local languageName = speedName

    table.insert(trackSpinner.placements, {
        name = languageName,
        data = {
            speed = speedValue,
            startCenter = false,
            spriteFolder = "danger/MaxHelpingHand/starSpinner", 
            particleColors = "EA64B7|3EE852,67DFEA|E85351,EA582C|33BDE8"
        }
    })
end

function trackSpinner.sprite(room, entity)
    local starfishTexture = entity.spriteFolder .. "/idle0_00"
    return drawableSpriteStruct.fromTexture(starfishTexture, entity)
end

return trackSpinner
