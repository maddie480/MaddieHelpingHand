local uiu = require("ui.utils")
local utils = require("utils")

-- with the power of jank OlympUI hooks, I shall make that max480 fellow disappear!
uiu.hook(utils, {
    humanizeVariableName = function(orig, name)
        if string.sub(name, 1, 14) ~= "MaxHelpingHand" then
            return orig(name)
        end
        -- render MaxHelpingHand as "Maddie Helping Hand"
        return "Maddie" .. string.sub(orig(name), 4)
    end
})
