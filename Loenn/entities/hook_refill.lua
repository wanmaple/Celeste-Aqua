local HookRefill = {}

HookRefill.name = "Aqua/Hook Refill"
HookRefill.depth = -100
HookRefill.justification = { 0.5, 0.5 }
HookRefill.texture = "objects/refill_hook/idle00"
HookRefill.placements = {
    name = "Hook Refill",
    data = {
        oneUse = true,
    },
}

return HookRefill