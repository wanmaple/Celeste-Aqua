local DrawableSprite = require("structs.drawable_sprite")
local DrawableText = require("structs.drawable_text")

local CustomGrapplingRefill = {}

CustomGrapplingRefill.name = "Aqua/Custom Grappling Refill"
CustomGrapplingRefill.depth = -100
CustomGrapplingRefill.justification = { 0.5, 0.5 }
CustomGrapplingRefill.fieldInformation = {
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
    refill_count = {
        fieldType = "integer",
    },
}
CustomGrapplingRefill.placements = {
    name = "Grappling Refill (Custom Count)",
    data = {
        oneUse = false,
        hookable = true,
        respawn_time = 2.5,
        refill_sprite = "",
        outline_texture = "",
        particle_color1 = "909cb0",
        particle_color2 = "515672",
        use_default_sprite = true,
        refill_count = 3,
        cap = -1,
        capped = false,
        always_refill = false,
    },
}

function CustomGrapplingRefill.sprite(room, entity)
    local x, y = entity.x, entity.y
    local texture = "objects/refills/refill_Hook/idle00"
    local refillSprite = DrawableSprite.fromTexture(texture, entity)
    local width, height = 8.0, 8.0
    local textNum = DrawableText.fromText(tostring(entity.refill_count), x, y, width, height, nil, 1.0)
    return { refillSprite, textNum, }
end

return CustomGrapplingRefill