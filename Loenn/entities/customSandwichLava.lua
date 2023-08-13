local lavaSandwich = {}

lavaSandwich.name = "MaxHelpingHand/CustomSandwichLava"
lavaSandwich.depth = 0
lavaSandwich.placements = {
    name = "lava_sandwich",
    data = {
        direction = "CoreModeBased",
        speed = 20.0,
        sandwichGap = 160.0,
        hotSurfaceColor = "ff8933",
        hotEdgeColor = "f25e29",
        hotCenterColor = "d01c01",
        coldSurfaceColor = "33ffe7",
        coldEdgeColor = "4ca2eb",
        coldCenterColor = "0151d0",
        flag = ""
    }
}

lavaSandwich.fieldInformation = {
    hotSurfaceColor = {
        fieldType = "color"
    },
    hotEdgeColor = {
        fieldType = "color"
    },
    hotCenterColor = {
        fieldType = "color"
    },
    coldSurfaceColor = {
        fieldType = "color"
    },
    coldEdgeColor = {
        fieldType = "color"
    },
    coldCenterColor = {
        fieldType = "color"
    },
    direction = {
        options = {
            ["Always Up"] = "AlwaysUp",
            ["Always Down"] = "AlwaysDown",
            ["Based on Core Mode"] = "CoreModeBased"
        },
        editable = false
    }
}

function lavaSandwich.texture(room, entity)
    if entity.direction == "AlwaysUp" then
        return "ahorn/MaxHelpingHand/lava_sandwich_up"
    elseif entity.direction == "AlwaysDown" then
        return "ahorn/MaxHelpingHand/lava_sandwich_down"
    else
        return "@Internal@/lava_sandwich"
    end
end

return lavaSandwich
