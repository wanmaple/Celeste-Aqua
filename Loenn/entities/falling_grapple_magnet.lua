local DrawableSprite = require("structs.drawable_sprite")

local FallingGrappleMagnet = {}

FallingGrappleMagnet.name = "Aqua/Falling Grapple Magnet"
FallingGrappleMagnet.depth = 8995
FallingGrappleMagnet.justification = { 0.5, 0.5, }
FallingGrappleMagnet.fieldInformation = {
    radius_in_tiles = {
        fieldType = "integer",
    },
}
FallingGrappleMagnet.placements = {
    name = "Falling Grapple Magnet",
    data = {
        radius_in_tiles = 4,
        flag = "Magnet1",
        on = true,
        sprite = "Aqua_GrappleMagnet",
        use_default_sprite = true,
        fall_delay = 0.6,
        fall_flag = "",
        grapple_trigger = true,
        flag_trigger = false,
    },
}

function FallingGrappleMagnet.sprite(room, entity)
    local magnetTexture = "objects/hook_magnet/base/idle00"
    local indicatorTexture = "objects/hook_magnet/normal/idle00"
    local rangeTexture = "objects/hook_magnet/circle_in_leonn"
    local magnet = DrawableSprite.fromTexture(magnetTexture, entity)
    local indicator = DrawableSprite.fromTexture(indicatorTexture, entity)
    local range = DrawableSprite.fromTexture(rangeTexture, entity)
    local radiusInTiles = math.max(math.min(entity.radius_in_tiles, 8), 2)
    range:setScale(radiusInTiles * 2, radiusInTiles * 2)
    range:setColor("007bfe7b")
    local sprites = { range, magnet, indicator, }
    return sprites
end

return FallingGrappleMagnet