local AquaRefill = {}

AquaRefill.name = "Aqua/Aqua Refill"
AquaRefill.depth = -100
AquaRefill.fieldInformation = {
    particle_color1 = {
        fieldType = "color",
        useAlpha = "false",
    },
    particle_color2 = {
        fieldType = "color",
        useAlpha = "false",
    },
}
AquaRefill.placements = {
    {
        name = "Refill (One Dash)",
        data = {
            twoDash = false,
            oneUse = false,
            hookable = true,
            respawn_time = 2.5,
            fill_stamina = true,
            refill_sprite = "",
            outline_texture = "",
            particle_color1 = "f19310",
            particle_color2 = "824c00",
            use_default_sprite = true,
        },
    },
    {
        name = "Refill (Two Dashes)",
        data = {
            twoDash = true,
            oneUse = false,
            hookable = true,
            respawn_time = 2.5,
            fill_stamina = true,
            refill_sprite = "",
            outline_texture = "",
            particle_color1 = "912ed4",
            particle_color2 = "4b1680",
            use_default_sprite = true,
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