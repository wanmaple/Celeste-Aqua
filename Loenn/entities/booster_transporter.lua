local DrawableSprite = require("structs.drawable_sprite")

local BoosterTransporter = {}

BoosterTransporter.name = "Aqua/Booster Transporter"
BoosterTransporter.depth = 8995
BoosterTransporter.justification = { 0.5, 0.5, }
BoosterTransporter.nodeLimits = {1, 1}
BoosterTransporter.placements = {
    name = "Booster Transporter",
    data = {
        absorb_duration = 0.2,
        release_duration = 0.2,
        transport_duration = 0.6,
        change_respawn = false,
    },
}

function BoosterTransporter.sprite(room, entity)
    local rangeTexture = "objects/hook_magnet/circle_in_leonn"
    local range = DrawableSprite.fromTexture(rangeTexture, entity)
    local radiusInTiles = 1
    range:setScale(radiusInTiles * 2, radiusInTiles * 2)
    range:setColor("007bfe7b")
    return range
end

return BoosterTransporter