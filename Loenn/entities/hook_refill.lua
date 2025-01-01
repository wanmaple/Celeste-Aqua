local HookRefill = {}

HookRefill.name = "Aqua/Hook Refill"
HookRefill.depth = -100
HookRefill.justification = { 0.5, 0.5 }
HookRefill.placements = {
    {
        name = "Hook Refill (One Charge)",
        data = {
            oneUse = true,
            chargeTwo = false,
        },
    },
    {
        name = "Hook Refill (Two Charge)",
        data = {
            oneUse = true,
            chargeTwo = true,
        },
    }
}

function HookRefill.texture(room, entity)
    return entity.chargeTwo and "objects/refills/refillTwo_Hook/idle00" or "objects/refills/refill_Hook/idle00"
end

return HookRefill