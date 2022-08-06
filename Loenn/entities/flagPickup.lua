local flagPickup = {}

flagPickup.name = "MaxHelpingHand/FlagPickup"
flagPickup.depth = 0
flagPickup.placements = {
    name = "flag_pickup",
    data = {
        appearOnFlag = "",
        flagOnPickup = "",
        collectFlag = "",
        spriteName = "MaxHelpingHand_FlagPickup_Flag",
        collectSound = "event:/game/general/seed_touch",
        allowRespawn = false
    }
}

flagPickup.fieldInformation = {
    spriteName = {
        options = { "MaxHelpingHand_FlagPickup_Coin", "MaxHelpingHand_FlagPickup_Flag" }
    }
}

function flagPickup.texture(room, entity)
    if entity.spriteName == "MaxHelpingHand_FlagPickup_Coin" then
        return "MaxHelpingHand/flagpickup/Coin/Coin0"
    else
        return "MaxHelpingHand/flagpickup/Flag/Flag0"
    end

end

return flagPickup
