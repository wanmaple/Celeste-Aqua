local DrawableSprite = require("structs.drawable_sprite")
local DrawableLine = require("structs.drawable_line")
local Utils = require("utils")

local GrappleMagnetZipper = {}

local COG_TEXTURES = {
    "objects/zipmover/cog",
    "objects/zipmover/moon/cog",
}

GrappleMagnetZipper.name = "Aqua/Grapple Magnet Zipper"
GrappleMagnetZipper.depth = 8995
GrappleMagnetZipper.justification = { 0.5, 0.5, }
GrappleMagnetZipper.nodeLineRenderType = "line"
GrappleMagnetZipper.nodeLimits = {1, -1}
GrappleMagnetZipper.fieldInformation = {
    radius_in_tiles = {
        fieldType = "integer",
    },
    speed_multipliers = {
        fieldType = "list",
    },
    delays = {
        fieldType = "list",
    },
    move_flags = {
        fieldType = "list",
    },
    cog_texture = {
        options = COG_TEXTURES,
        editable = true,
    },
    chain_color = {
        fieldType = "color",
        useAlpha = false,
    },
    chain_light_color = {
        fieldType = "color",
        useAlpha = false,
    },
}
GrappleMagnetZipper.placements = {
    name = "Grapple Magnet Zipper",
    data = {
        radius_in_tiles = 4,
        flag = "Magnet1",
        on = true,
        sprite = "Aqua_GrappleMagnet",
        use_default_sprite = true,
        speed_multipliers = "1.0",
        delays = "0.2",
        move_flags = "",
        return_speed_multiplier = 1.0,
        delay_before_return = 0.5,
        grapple_trigger = true,
        flag_trigger = false,
        one_use = false,
        cog_texture = "objects/zipmover/cog",
        chain_color = "663931",
        chain_light_color = "9b6157",
    },
}

local function addNodeSprites(sprites, entity)
    local nodes = entity.nodes or {}
    local cogTexture = entity.cog_texture or "objects/zipmover/cog"
    local chainColor = entity.chain_color
    local selfCog = DrawableSprite.fromTexture(cogTexture, entity)
    selfCog:setJustification(0.5, 0.5)
    local points = { entity.x, entity.y, }
    for _, node in ipairs(nodes) do
        table.insert(points, node.x)
        table.insert(points, node.y)
    end
    local leftLine = DrawableLine.fromPoints(points, chainColor, 1)
    local rightLine = DrawableLine.fromPoints(points, chainColor, 1)
    leftLine:setOffset(0, 4.5)
    rightLine:setOffset(0, -4.5)
    for _, sprite in ipairs(leftLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    for _, sprite in ipairs(rightLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end
    table.insert(sprites, selfCog)
    for _, node in ipairs(nodes) do
        local nodeCog = DrawableSprite.fromTexture(cogTexture, { x = node.x, y = node.y, })
        nodeCog:setJustification(0.5, 0.5)
        table.insert(sprites, nodeCog)
    end
end

function GrappleMagnetZipper.sprite(room, entity)
    local sprites = {}
    addNodeSprites(sprites, entity)
    local magnetTexture = entity.on and "objects/hook_magnet/base/idle00" or "objects/hook_magnet/base/close07"
    local indicatorTexture = entity.on and "objects/hook_magnet/normal/idle00" or "objects/hook_magnet/normal/close07"
    local rangeTexture = "objects/hook_magnet/circle_in_leonn"
    local magnet = DrawableSprite.fromTexture(magnetTexture, entity)
    local indicator = DrawableSprite.fromTexture(indicatorTexture, entity)
    local range = DrawableSprite.fromTexture(rangeTexture, entity)
    local radiusInTiles = math.max(math.min(entity.radius_in_tiles, 8), 2)
    range:setScale(radiusInTiles * 2, radiusInTiles * 2)
    range:setColor("007bfe7b")
    table.insert(sprites, range)
    table.insert(sprites, magnet)
    table.insert(sprites, indicator)
    return sprites
end

function GrappleMagnetZipper.nodeSprite(room, entity, node, nodeIndex)
    local magnetTexture = entity.on and "objects/hook_magnet/base/idle00" or "objects/hook_magnet/base/close07"
    local indicatorTexture = entity.on and "objects/hook_magnet/normal/idle00" or "objects/hook_magnet/normal/close07"
    local rangeTexture = "objects/hook_magnet/circle_in_leonn"
    local magnet = DrawableSprite.fromTexture(magnetTexture, node)
    local indicator = DrawableSprite.fromTexture(indicatorTexture, node)
    local range = DrawableSprite.fromTexture(rangeTexture, node)
    local radiusInTiles = math.max(math.min(entity.radius_in_tiles, 8), 2)
    range:setScale(radiusInTiles * 2, radiusInTiles * 2)
    range:setColor("007bfe7b")
    return { range, magnet, indicator, }
end

function GrappleMagnetZipper.selection(room, entity)
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

return GrappleMagnetZipper