local AquaRefill = {}

AquaRefill.name = "Aqua/Aqua Refill"
AquaRefill.depth = -100
AquaRefill.placements = {
    {
        name = "Refill (One Dash)",
        data = {
            twoDash = false,
            oneUse = false,
            hookable = true,
        },
    },
    {
        name = "Refill (Two Dashes)",
        data = {
            twoDash = true,
            oneUse = false,
            hookable = true,
        },
    },
}

function AquaRefill.texture(room, entity)
    if entity.hookable then
        return entity.twoDash and "objects/refills/refillTwo_Hookable/idle00" or "objects/refills/refillOne_Hookable/idle00"
    end
    return entity.twoDash and "objects/refillTwo/idle00" or "objects/refill/idle00"
end

return AquaRefill