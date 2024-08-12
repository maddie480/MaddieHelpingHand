local mods = require("mods")
local generateTriggerName = mods.requireFromPlugin("libraries.triggerRenamer")

local trigger = {}

trigger.name = "MaxHelpingHand/CameraOffsetBorder"
trigger.category = "camera"
trigger.triggerText = generateTriggerName
trigger.placements = {
    name = "trigger",
    data = {
        topLeft = true,
        topCenter = true,
        topRight = true,
        centerLeft = true,
        centerRight = true,
        bottomLeft = true,
        bottomCenter = true,
        bottomRight = true,
        flag = "",
        inside = false,
        inverted = false
    }
}

trigger.fieldOrder = {"x", "y", "width", "height", "topLeft", "topCenter", "topRight", "centerLeft", "centerRight", "bottomLeft", "bottomCenter", "bottomRight", "flag", "inverted", "inside"}

return trigger