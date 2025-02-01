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
    particle_color = {
        fieldType = "color",
        useAlpha = false,
    },
}
AquaBooster.placements = {
    {
        name = "Booster (Orange)",
        data = {
            red = false,
            hookable = true,
            sprite = "Aqua_BoosterOrange",
            particle_color = "bc630e",
            use_default_sprite = true,
        },
    },
    {
        name = "Booster (Purple)",
        data = {
            red = true,
            hookable = true,
            sprite = "Aqua_BoosterPurple",
            particle_color = "760ebc",
            use_default_sprite = true,
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