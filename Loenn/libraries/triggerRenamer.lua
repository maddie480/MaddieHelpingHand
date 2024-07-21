local triggers = require("triggers")

local function renderTriggerName(_, trigger)
    local triggerName = triggers.getDrawableDisplayText(trigger)
    if string.sub(triggerName, 1, 17) == "Max Helping Hand " then
        return "Maddie" .. string.sub(triggerName, 4)
    else
        return triggerName
    end
end

return renderTriggerName
