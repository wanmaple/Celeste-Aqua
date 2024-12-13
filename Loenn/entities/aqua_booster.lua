local AquaBooster = {}

AquaBooster.name = "Aqua/Aqua Booster"
AquaBooster.depth = -8500
AquaBooster.placements = {
    {
        name = "Booster (Orange)",
        data = {
            red = false,
            hookable = true,
        },
    },
    {
        name = "Booster (Purple)",
        data = {
            red = true,
            hookable = true,
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