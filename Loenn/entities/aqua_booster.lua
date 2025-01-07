local AquaBooster = {}

local INTERNAL_SKINS = {
    "Aqua_BoosterOrange",
    "Aqua_BoosterPurple",
}

AquaBooster.name = "Aqua/Aqua Booster"
AquaBooster.depth = -8500
AquaBooster.fieldInformation = {
    sprite = {
        options = INTERNAL_SKINS,
        editable = true,
    },
}
AquaBooster.placements = {
    {
        name = "Booster (Orange)",
        data = {
            red = false,
            hookable = true,
            sprite = "Aqua_BoosterOrange",
        },
    },
    {
        name = "Booster (Purple)",
        data = {
            red = true,
            hookable = true,
            sprite = "Aqua_BoosterPurple",
        },
    },
}

function AquaBooster.texture(room, entity)
    if entity.hookable then
        return entity.red and "objects/booster_purple/booster_red00" or "objects/booster_orange/booster00"
    end
    return entity.red and "objects/booster/boosterRed00" or "objects/booster/booster00"
end

return AquaBooster