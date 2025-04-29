local DrawableSprite = require("structs.drawable_sprite")

local BoosterTransporter = {}

local SPRITES = {
    "Aqua_BoosterTransporterCustom",
    "Aqua_BoosterTransporterBlue",
    "Aqua_BoosterTransporterGreen",
    "Aqua_BoosterTransporterRed",
    "Aqua_BoosterTransporterYellow",
}

BoosterTransporter.name = "Aqua/Booster Transporter"
BoosterTransporter.depth = 8995
BoosterTransporter.justification = { 0.5, 0.5, }
BoosterTransporter.nodeLimits = {1, 1}
BoosterTransporter.fieldInformation = {
    sprite = {
        options = SPRITES,
        editable = true,
    },
    color = {
        fieldType = "color",
        useAlpha = false,
    },
    particle_color = {
        fieldType = "color",
        useAlpha = false,
    },
}
BoosterTransporter.placements = {
    name = "Booster Transporter",
    data = {
        absorb_duration = 0.2,
        release_duration = 0.2,
        transport_duration = 0.6,
        change_respawn = false,
        in_base_texture = "objects/booster_transporter/in_base00",
        out_base_texture = "objects/booster_transporter/out_base",
        sprite = "Aqua_BoosterTransporterCustom",
        color = "ffffff",
        particle_color = "808080",
        use_default_sprite = true,
    },
}

local DEFAULT_IN_BASE = "objects/booster_transporter/in_base00"
local DEFAULT_OUT_BASE = "objects/booster_transporter/out_base00"

function BoosterTransporter.sprite(room, entity)
    local baseTexture = entity.use_default_sprite and DEFAULT_IN_BASE or entity.in_base_texture
    local inBase = DrawableSprite.fromTexture(baseTexture, entity)
    return inBase
end

function BoosterTransporter.nodeSprite(room, entity, node, nodeIndex)
    local baseTexture = entity.use_default_sprite and DEFAULT_OUT_BASE or (entity.out_base_texture .. "00")
    local outBase = DrawableSprite.fromTexture(baseTexture, node)
    return outBase
end

return BoosterTransporter