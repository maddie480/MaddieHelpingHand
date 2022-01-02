local defaultMemorialTexture = "scenery/memorial/memorial"

local memorial = {}

memorial.name = "MaxHelpingHand/CustomMemorialWithDreamingAttribute"
memorial.depth = 100
memorial.justification = {0.5, 1.0}
memorial.placements = {
    {
        name = "not_dreaming",
        data = {
            dreaming = false,
            dialog = "MEMORIAL",
            sprite = defaultMemorialTexture,
            spacing = 16
        }
    },
    {
        name = "dreaming",
        data = {
            dreaming = true,
            dialog = "MEMORIAL",
            sprite = defaultMemorialTexture,
            spacing = 16
        }
    }
}

function memorial.texture(room, entity)
    return entity.sprite or defaultMemorialTexture
end

return memorial
