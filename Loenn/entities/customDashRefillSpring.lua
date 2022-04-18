local springDepth = -8501

local function springTexture(room, entity)
    return entity.spriteDirectory .. "/00"
end

local springUp = {}

springUp.name = "MaxHelpingHand/CustomDashRefillSpring"
springUp.depth = springDepth
springUp.justification = {0.5, 1.0}
springUp.texture = springTexture
springUp.placements = {
    name = "up",
    data = {
        spriteDirectory = "objects/MaxHelpingHand/twoDashRefillSpring",
        playerCanUse = true,
        ignoreLighting = false,
        dashCount = 2,
        dashCountCap = 2,
        mode = "Set"
    }
}
springUp.fieldInformation = {
    mode = {
        options = { "Set", "Add", "AddCapped" },
        editable = false
    },
    dashCount = {
        fieldType = "integer"
    },
    dashCountCap = {
        fieldType = "integer"
    }
}

local springRight = {}

springRight.name = "MaxHelpingHand/CustomDashRefillSpringLeft"
springRight.depth = springDepth
springRight.justification = {0.5, 1.0}
springRight.texture = springTexture
springRight.rotation = math.pi / 2
springRight.placements = {
    name = "right",
    data = {
        spriteDirectory = "objects/MaxHelpingHand/twoDashRefillSpring",
        ignoreLighting = false,
        dashCount = 2,
        dashCountCap = 2,
        mode = "Set"
    }
}
springRight.fieldInformation = {
    mode = {
        options = { "Set", "Add", "AddCapped" },
        editable = false
    },
    dashCount = {
        fieldType = "integer"
    },
    dashCountCap = {
        fieldType = "integer"
    }
}

local springLeft = {}

springLeft.name = "MaxHelpingHand/CustomDashRefillSpringRight"
springLeft.depth = springDepth
springLeft.justification = {0.5, 1.0}
springLeft.texture = springTexture
springLeft.rotation = -math.pi / 2
springLeft.placements = {
    name = "left",
    data = {
        spriteDirectory = "objects/MaxHelpingHand/twoDashRefillSpring",
        ignoreLighting = false,
        dashCount = 2,
        dashCountCap = 2,
        mode = "Set"
    }
}
springLeft.fieldInformation = {
    mode = {
        options = { "Set", "Add", "AddCapped" },
        editable = false
    },
    dashCount = {
        fieldType = "integer"
    },
    dashCountCap = {
        fieldType = "integer"
    }
}

return {
    springUp,
    springRight,
    springLeft
}
