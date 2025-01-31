local GrapplingRefill = {}

GrapplingRefill.name = "Aqua/Grappling Refill"
GrapplingRefill.depth = -100
GrapplingRefill.justification = { 0.5, 0.5 }
GrapplingRefill.fieldInformation = {
    particle_color1 = {
        fieldType = "color",
        useAlpha = "false",
    },
    particle_color2 = {
        fieldType = "color",
        useAlpha = "false",
    },
    cap = {
        fieldType = "integer",
    },
}
GrapplingRefill.placements = {
    {
        name = "Grappling Refill (One Charge)",
        data = {
            chargeTwo = false,
            oneUse = false,
            hookable = true,
            respawn_time = 2.5,
            refill_sprite = "",
            outline_texture = "",
            particle_color1 = "909cb0",
            particle_color2 = "515672",
            use_default_sprite = true,
            cap = -1,
            capped = false,
            always_refill = false,
        },
    },
    {
        name = "Grappling Refill (Two Charge)",
        data = {
            chargeTwo = true,
            oneUse = false,
            hookable = true,
            respawn_time = 2.5,
            refill_sprite = "",
            outline_texture = "",
            particle_color1 = "d0bee9",
            particle_color2 = "8e7ca6",
            use_default_sprite = true,
            cap = -1,
            capped = false,
            always_refill = false,
        },
    }
}

function GrapplingRefill.texture(room, entity)
    return entity.chargeTwo and "objects/refills/refillTwo_Hook/idle00" or "objects/refills/refill_Hook/idle00"
end

return GrapplingRefill