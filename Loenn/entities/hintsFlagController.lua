local hintsFlagController = {}

hintsFlagController.name = "MaxHelpingHand/HintsFlagController"
hintsFlagController.depth = 0
hintsFlagController.placements = {
    name = "hints_flag_controller",
    data = {
        outputFlag = "hints",
        ["not"] = false
    }
}

function hintsFlagController.texture(room, entity)
    if entity["not"] then
        return "ahorn/MaxHelpingHand/hints_flag_controller_inv"
    else
        return "ahorn/MaxHelpingHand/hints_flag_controller"
    end
end

return hintsFlagController
