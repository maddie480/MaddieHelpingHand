module MaxHelpingHandCustomSummitCheckpoint

using ..Ahorn, Maple

@mapdef Entity "MaxHelpingHand/CustomSummitCheckpoint" CustomSummitCheckpoint(x::Integer, y::Integer, firstDigit::String="zero", secondDigit::String="zero",
    spriteDirectory::String="MaxHelpingHand/summitcheckpoints", confettiColors::String="fe2074,205efe,cefe20")

const placements = Ahorn.PlacementDict(
    "Custom Summit Checkpoint (max480's Helping Hand)" => Ahorn.EntityPlacement(
        CustomSummitCheckpoint
    )
)

const numberlist = String["zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "minus", "questionmark"]

Ahorn.editingOptions(entity::CustomSummitCheckpoint) = Dict{String, Any}(
    "firstDigit" => numberlist,
    "secondDigit" => numberlist
)

function Ahorn.selection(entity::CustomSummitCheckpoint)
    directory = get(entity.data, "spriteDirectory", "MaxHelpingHand/summitcheckpoints")
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle("$directory/base02.png", x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomSummitCheckpoint, room::Maple.Room)
    directory = get(entity.data, "spriteDirectory", "MaxHelpingHand/summitcheckpoints")
    digit1 = get(entity.data, "firstDigit", 0)
    digit2 = get(entity.data, "secondDigit", 0)

    Ahorn.drawSprite(ctx, "$directory/base02.png", 0, 0)
    Ahorn.drawSprite(ctx, "$directory/$digit1/numberbg.png", -2, 4)
    Ahorn.drawSprite(ctx, "$directory/$digit1/number.png", -2, 4)
    Ahorn.drawSprite(ctx, "$directory/$digit2/numberbg.png", 2, 4)
    Ahorn.drawSprite(ctx, "$directory/$digit2/number.png", 2, 4)
end

end