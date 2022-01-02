local utils = require("utils")

local barrier = {}

barrier.name = "MaxHelpingHand/KevinBarrier"
barrier.depth = 0
function barrier.color(room, entity)
    local color  = {0.25, 0.25, 0.25, 0.8}

    if entity.color then
        local success, r, g, b = utils.parseHexColor(entity.color)

        if success then
            color = {r, g, b}
        end
    end

    return color
end

barrier.placements = {
    name = "barrier",
    data = {
        width = 8,
        height = 8,
        color = "62222b",
        particleColor = "ffffff",
        flashOnHit = true,
        invisible = false
    }
}

barrier.fieldInformation = {
    color = {
        fieldType = "color"
    },
    particleColor = {
        fieldType = "color"
    }
}

return barrier
