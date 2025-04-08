local DrawableSprite = require("structs.drawable_sprite")
local Utils = require("utils")

local UnhookableIceWall = {}

UnhookableIceWall.name = "Aqua/Unhookable Ice Wall"
UnhookableIceWall.depth = 1999
UnhookableIceWall.canResize = { false, true, }
UnhookableIceWall.placements = {
    {
        name = "Unhookable Ice Wall (Left)",
        placementType = "rectangle",
        data = {
            height = 8,
            left = false,
            attach_to_solid = false,
        }
    },
    {
        name = "Unhookable Ice Wall (Right)",
        placementType = "rectangle",
        data = {
            height = 8,
            left = true,
            attach_to_solid = false,
        }
    },
}
local iceTopTexture = "objects/wallBooster/iceTop00"
local iceMiddleTexture = "objects/wallBooster/iceMid00"
local iceBottomTexture = "objects/wallBooster/iceBottom00"

local function getWallTextures(entity)
    return iceTopTexture, iceMiddleTexture, iceBottomTexture
end

function UnhookableIceWall.sprite(room, entity)
    local sprites = {}

    local left = entity.left
    local height = entity.height or 8
    local tileHeight = math.floor(height / 8)
    local offsetX = left and 0 or 8
    local scaleX = left and 1 or -1

    local topTexture, middleTexture, bottomTexture = getWallTextures(entity)

    for i = 2, tileHeight - 1 do
        local middleSprite = DrawableSprite.fromTexture(middleTexture, entity)

        middleSprite:addPosition(offsetX, (i - 1) * 8)
        middleSprite:setScale(scaleX, 1)
        middleSprite:setJustification(0.0, 0.0)

        table.insert(sprites, middleSprite)
    end

    local topSprite = DrawableSprite.fromTexture(topTexture, entity)
    local bottomSprite = DrawableSprite.fromTexture(bottomTexture, entity)

    topSprite:addPosition(offsetX, 0)
    topSprite:setScale(scaleX, 1)
    topSprite:setJustification(0.0, 0.0)

    bottomSprite:addPosition(offsetX, (tileHeight - 1) * 8)
    bottomSprite:setScale(scaleX, 1)
    bottomSprite:setJustification(0.0, 0.0)

    table.insert(sprites, topSprite)
    table.insert(sprites, bottomSprite)

    return sprites
end

function UnhookableIceWall.rectangle(room, entity)
    return Utils.rectangle(entity.x, entity.y, 8, entity.height or 8)
end

function UnhookableIceWall.flip(room, entity, horizontal, vertical)
    if horizontal then
        entity.left = not entity.left
        entity.x += (entity.left and 8 or -8)
    end

    return horizontal
end

return UnhookableIceWall