local DrawableSprite = require("structs.drawable_sprite")
local DrawableLine = require("structs.drawable_line")
local DrawableNinePatch = require("structs.drawable_nine_patch")
local DrawableRectangle = require("structs.drawable_rectangle")
local Utils = require("utils")

local RodZipMover = {}

local themeTextures = {
    normal = {
        nodeCog = "objects/zipmover/cog",
        lights = "objects/zipmover/light01",
        block = "objects/zipmover/block",
        innerCogs = "objects/zipmover/innercog"
    },
    moon = {
        nodeCog = "objects/zipmover/moon/cog",
        lights = "objects/zipmover/moon/light01",
        block = "objects/zipmover/moon/block",
        innerCogs = "objects/zipmover/moon/innercog"
    }
}

local blockNinePatchOptions = {
    mode = "border",
    borderMode = "repeat"
}

local centerColor = {0, 0, 0}
local ropeColor = {102 / 255, 57 / 255, 49 / 255}

local themes = {
    "Normal", "Moon"
}

RodZipMover.name = "Aqua/Rod Zip Mover"
RodZipMover.depth = -9999
RodZipMover.nodeVisibility = "never"
RodZipMover.nodeLimits = {1, 1}
RodZipMover.warnBelowSize = {16, 16}
RodZipMover.fieldInformation = {
    group = {
        fieldType = "integer",
    },
    theme = {
        options = themes,
        editable = false
    },
}
RodZipMover.placements = {}

for i, theme in ipairs(themes) do
    RodZipMover.placements[i] = {
        name = "Rod Zip Mover (" .. theme .. ")",
        data = {
            width = 16,
            height = 16,
            flag = "Rod1",
            duration = 0.5,
            theme = theme,
            hue_offset = 0.0,
            saturation_offset = 0.0,
        },
    }
end

local function addNodeSprites(sprites, entity, cogTexture, centerX, centerY, centerNodeX, centerNodeY)
    local nodeCogSprite = DrawableSprite.fromTexture(cogTexture, entity)

    nodeCogSprite:setPosition(centerNodeX, centerNodeY)
    nodeCogSprite:setJustification(0.5, 0.5)

    local points = {centerX, centerY, centerNodeX, centerNodeY}
    local leftLine = DrawableLine.fromPoints(points, ropeColor, 1)
    local rightLine = DrawableLine.fromPoints(points, ropeColor, 1)

    leftLine:setOffset(0, 4.5)
    rightLine:setOffset(0, -4.5)

    leftLine.depth = 5000
    rightLine.depth = 5000

    for _, sprite in ipairs(leftLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    for _, sprite in ipairs(rightLine:getDrawableSprite()) do
        table.insert(sprites, sprite)
    end

    table.insert(sprites, nodeCogSprite)
end

local function addBlockSprites(sprites, entity, blockTexture, lightsTexture, x, y, width, height)
    local rectangle = DrawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, centerColor)

    local frameNinePatch = DrawableNinePatch.fromTexture(blockTexture, blockNinePatchOptions, x, y, width, height)
    local frameSprites = frameNinePatch:getDrawableSprite()

    local lightsSprite = DrawableSprite.fromTexture(lightsTexture, entity)

    lightsSprite:addPosition(math.floor(width / 2), 0)
    lightsSprite:setJustification(0.5, 0.0)

    table.insert(sprites, rectangle:getDrawableSprite())

    for _, sprite in ipairs(frameSprites) do
        table.insert(sprites, sprite)
    end

    table.insert(sprites, lightsSprite)
end

function RodZipMover.sprite(room, entity)
    local sprites = {}

    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 16, entity.height or 16
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y

    local centerX, centerY = x + halfWidth, y + halfHeight
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    local theme = string.lower(entity.theme or "normal")
    local themeData = themeTextures[theme] or themeTextures["normal"]

    addNodeSprites(sprites, entity, themeData.nodeCog, centerX, centerY, centerNodeX, centerNodeY)
    addBlockSprites(sprites, entity, themeData.block, themeData.lights, x, y, width, height)

    return sprites
end

function RodZipMover.selection(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 8, entity.height or 8
    local halfWidth, halfHeight = math.floor(entity.width / 2), math.floor(entity.height / 2)

    local nodes = entity.nodes or {{x = 0, y = 0}}
    local nodeX, nodeY = nodes[1].x, nodes[1].y
    local centerNodeX, centerNodeY = nodeX + halfWidth, nodeY + halfHeight

    local theme = string.lower(entity.theme or "normal")
    local themeData = themeTextures[theme] or themeTextures["normal"]

    local cogSprite = DrawableSprite.fromTexture(themeData.nodeCog, entity)
    local cogWidth, cogHeight = cogSprite.meta.width, cogSprite.meta.height

    local mainRectangle = Utils.rectangle(x, y, width, height)
    local nodeRectangle = Utils.rectangle(centerNodeX - math.floor(cogWidth / 2), centerNodeY - math.floor(cogHeight / 2), cogWidth, cogHeight)

    return mainRectangle, {nodeRectangle}
end

return RodZipMover