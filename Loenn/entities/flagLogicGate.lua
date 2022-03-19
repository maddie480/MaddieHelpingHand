local flagLogicGate = {}

flagLogicGate.name = "MaxHelpingHand/FlagLogicGate"
flagLogicGate.depth = 0
flagLogicGate.placements = {
    name = "flag_logic_gate",
    data = {
        inputFlags = "flag1,!flag2,flag3",
        outputFlag = "flag4",
        func = "AND",
        ["not"] = false
    }
}

flagLogicGate.fieldInformation = {
    func = {
        options = { "AND", "OR", "XOR" },
        editable = false
    }
}

function flagLogicGate.texture(room, entity)
    text = entity.func
    if entity["not"] then
        if entity.func == "XOR" then
            -- this one is known as XNOR in electronics, not NXOR, so...
            text = "XNOR"
        else
            text = "N" .. entity.func
        end
    end

    return "ahorn/MaxHelpingHand/flag_logic_gate_" .. text
end

return flagLogicGate
