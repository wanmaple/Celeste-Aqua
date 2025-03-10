local DrawableSprite = require("structs.drawable_sprite")

local MoveGrappleMagnet = {}

local MOVE_DIRECTIONS = {
    "Up",
    "Down",
    "Left",
    "Right",
}

MoveGrappleMagnet.name = "Aqua/Move Grapple Magnet"
MoveGrappleMagnet.depth = 8995
MoveGrappleMagnet.justification = { 0.5, 0.5, }
MoveGrappleMagnet.fieldInformation = {
    radius_in_tiles = {
        fieldType = "integer",
    },
    direction = {
        options = MOVE_DIRECTIONS,
        editable = false,
    },
}
MoveGrappleMagnet.placements = {}

for _, dir in ipairs(MOVE_DIRECTIONS) do
    table.insert(MoveGrappleMagnet.placements, {
        name = string.format("Move Grapple Magnet (%s)", dir),
        data = {
            radius_in_tiles = 4,
            flag = "Magnet1",
            on = true,
            sprite = "Aqua_MoveGrappleMagnet" .. dir,
            use_default_sprite = true,
            direction = dir,
            acceleration = 300.0,
            move_speed = 60.0,
            move_flag = "",
            grapple_trigger = true,
            flag_trigger = false,
            one_use = false,
        },
    })
end

function MoveGrappleMagnet.sprite(room, entity)
    local magnetTexture = "objects/hook_magnet/base/idle00"
    local dirStr = string.lower(entity.direction)
    local indicatorTexture = string.format("objects/hook_magnet/move/%s/idle00", dirStr)
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

return MoveGrappleMagnet