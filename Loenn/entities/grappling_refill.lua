local GrapplingRefill = {}

GrapplingRefill.name = "Aqua/Grappling Refill"
GrapplingRefill.depth = -100
GrapplingRefill.justification = { 0.5, 0.5 }
GrapplingRefill.placements = {
    {
        name = "Grappling Refill (One Charge)",
        data = {
            oneUse = true,
            chargeTwo = false,
        },
    },
    {
        name = "Grappling Refill (Two Charge)",
        data = {
            oneUse = true,
            chargeTwo = true,
        },
    }
}

function GrapplingRefill.texture(room, entity)
    return entity.chargeTwo and "objects/refills/refillTwo_Hook/idle00" or "objects/refills/refill_Hook/idle00"
end

return GrapplingRefill