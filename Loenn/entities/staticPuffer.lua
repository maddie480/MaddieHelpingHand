local puffer = {}

puffer.name = "MaxHelpingHand/StaticPuffer"
puffer.depth = 0
puffer.texture = "objects/puffer/idle00"
puffer.placements = {
    {
        name = "left",
        data = {
            right = false,
            downboostTolerance = -1
        }
    },
    {
        name = "right",
        data = {
            right = true,
            downboostTolerance = -1
        }
    }
}

puffer.fieldInformation = {
    downboostTolerance = {
        fieldType = "integer"
    }
}

function puffer.scale(room, entity)
    local right = entity.right

    return right and 1 or -1, 1
end

return puffer
