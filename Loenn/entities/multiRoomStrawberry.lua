local strawberry = {}

strawberry.name = "MaxHelpingHand/MultiRoomStrawberry"
strawberry.depth = -100

function strawberry.texture(room, entity)
    local moon = entity.moon
    local winged = entity.winged

    if moon then
        if winged then
            return "collectables/moonBerry/ghost00"
        else
            return "collectables/moonBerry/normal00"
        end
    else
        if winged then
            return "collectables/strawberry/wings01"
        else
            return "collectables/strawberry/normal00"
        end
    end
end

strawberry.placements = {
    {
        name = "normal",
        data = {
            name = "multi_room_strawberry",
            winged = false,
            moon = false,
            checkpointID = -1,
            order = -1
        },
    },
    {
        name = "normal_winged",
        data = {
            name = "multi_room_strawberry",
            winged = true,
            moon = false,
            checkpointID = -1,
            order = -1
        },
    },
    {
        name = "moon",
        data = {
            name = "multi_room_strawberry",
            winged = false,
            moon = true,
            checkpointID = -1,
            order = -1
        },
    }
}

return strawberry
