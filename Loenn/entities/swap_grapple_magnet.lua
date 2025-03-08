local DrawableSprite = require("structs.drawable_sprite")
local DrawableNinePatch = require("structs.drawable_nine_patch")
local Utils = require("utils")

local SwapGrappleMagnet = {}

local FRAME_TEXTURES = {
    "objects/frames/frame1",
    "objects/frames/frame2",
}

SwapGrappleMagnet.name = "Aqua/Swap Grapple Magnet"
SwapGrappleMagnet.depth = 8995
SwapGrappleMagnet.justification = { 0.5, 0.5, }
SwapGrappleMagnet.nodeLineRenderType = "line"
SwapGrappleMagnet.nodeLimits = {1, 1}
SwapGrappleMagnet.fieldInformation = {
    radius_in_tiles = {
        fieldType = "integer",
    },
    frame_texture = {
        options = FRAME_TEXTURES,
        editable = true,
    },
}
SwapGrappleMagnet.placements = {
    name = "Swap Grapple Magnet",
    data = {
        radius_in_tiles = 4,
        flag = "Magnet1",
        on = true,
        sprite = "Aqua_SwapGrappleMagnet",
        use_default_sprite = true,
        swap_flag = "",
        use_flag_to_trig = false,
        frame_texture = "objects/frames/frame1",
    },
}

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

function SwapGrappleMagnet.sprite(room, entity)
    local sprites = {}
    local fromX, fromY = entity.x or 0, entity.y or 0
    local toX, toY = entity.nodes[1].x, entity.nodes[1].y
    local borderSprite = DrawableNinePatch.fromTexture(entity.frame_texture, ninePatchOptions, math.min(fromX, toX) - 8.0, math.min(fromY, toY) - 8.0, math.abs(toX - fromX) + 16.0, math.abs(toY - fromY) + 16.0)
    for _, sprite in ipairs(borderSprite:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    local magnetTexture = "objects/hook_magnet/idle00"
    local rangeTexture = "objects/hook_magnet/circle_in_leonn"
    local magnet = DrawableSprite.fromTexture(magnetTexture, entity)
    local range = DrawableSprite.fromTexture(rangeTexture, entity)
    local radiusInTiles = math.max(math.min(entity.radius_in_tiles, 8), 2)
    range:setScale(radiusInTiles * 2, radiusInTiles * 2)
    range:setColor("007bfe7b")
    table.insert(sprites, range)
    table.insert(sprites, magnet)
    return sprites
end

function SwapGrappleMagnet.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local radiusInTiles = math.max(math.min(entity.radius_in_tiles, 8), 2)
    local width, height = radiusInTiles * 8.0 * 2.0, radiusInTiles * 8.0 * 2.0
    local halfWidth, halfHeight = width * 0.5, height * 0.5
    local mainRect = Utils.rectangle(x - halfWidth, y - halfHeight, width, height)
    local nodes = entity.nodes or {}
    local nodeRects = {}
    for _, node in ipairs(nodes) do
        local rect = Utils.rectangle(node.x - halfWidth, node.y - halfHeight, width, height)
        table.insert(nodeRects, rect)
    end
    return mainRect, nodeRects
end

return SwapGrappleMagnet