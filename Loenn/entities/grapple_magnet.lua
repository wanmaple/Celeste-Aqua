local DrawableSprite = require("structs.drawable_sprite")

local GrappleMagnet = {}

GrappleMagnet.name = "Aqua/Grapple Magnet"
GrappleMagnet.depth = 8995
GrappleMagnet.justification = { 0.5, 0.5, }
GrappleMagnet.fieldInformation = {
    radius_in_tiles = {
        fieldType = "integer",
    },
}
GrappleMagnet.placements = {
    name = "Grapple Magnet",
    data = {
        radius_in_tiles = 4,
        flag = "Magnet1",
        on = true,
        sprite = "Aqua_GrappleMagnet",
        use_default_sprite = true,
    },
}

function GrappleMagnet.sprite(room, entity)
    local magnetTexture = "objects/hook_magnet/idle00"
    local rangeTexture = "objects/hook_magnet/circle_in_leonn"
    local magnet = DrawableSprite.fromTexture(magnetTexture, entity)
    local range = DrawableSprite.fromTexture(rangeTexture, entity)
    local radiusInTiles = math.max(math.min(entity.radius_in_tiles, 8), 2)
    range:setScale(radiusInTiles * 2, radiusInTiles * 2)
    range:setColor("007bfe7b")
    local sprites = { range, magnet, }
    return sprites
end

return GrappleMagnet