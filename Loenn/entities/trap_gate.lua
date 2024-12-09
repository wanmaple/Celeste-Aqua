local DrawableNinePatch = require("structs.drawable_nine_patch")
local DrawableSprite = require("structs.drawable_sprite")
local Enums = require("consts.celeste_enums")
local Utils = require("utils")
local Depths = require("consts.object_depths")

local TrapGate = {}

local textures = {
    "block", "mirror", "temple", "stars"
}
local textureOptions = {}

for _, texture in ipairs(textures) do
    textureOptions[Utils.titleCase(texture)] = texture
end

TrapGate.name = "Aqua/Trap Gate"
TrapGate.depth = 0
TrapGate.nodeLimits = {1, 1}
TrapGate.nodeLineRenderType = "line"
TrapGate.warnBelowSize = {16, 16}
TrapGate.fieldInformation = {
    sprite = {
        options = textureOptions
    },
    group = {
        fieldType = "integer",
    },
    color = {
        fieldType = "color",
        useAlpha = false,
    },
}
TrapGate.placements = {}

for i, texture in ipairs(textures) do
    TrapGate.placements[i] = {
        name = "Trap Gate (" .. texture .. ")",
        data = {
            width = 16,
            height = 16,
            sprite = texture,
            group = 0,
            close_time = 1.0,
            color = "ff0000",
        }
    }
end

local ninePatchOptions = {
    mode = "fill",
    borderMode = "repeat",
    fillMode = "repeat"
}

local frameTexture = "objects/switchgate/%s"
local middleTexture = "objects/switchgate/icon00"

function TrapGate.sprite(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24

    local blockSprite = entity.sprite or "block"
    local frame = string.format(frameTexture, blockSprite)

    local ninePatch = DrawableNinePatch.fromTexture(frame, ninePatchOptions, x, y, width, height)
    local middleSprite = DrawableSprite.fromTexture(middleTexture, entity)
    middleSprite:setColor(entity.color)
    local sprites = ninePatch:getDrawableSprite()

    middleSprite:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, middleSprite)

    return sprites
end

function TrapGate.selection(room, entity)
    local nodes = entity.nodes or {}
    local x, y = entity.x or 0, entity.y or 0
    local nodeX, nodeY = nodes[1].x or x, nodes[1].y or y
    local width, height = entity.width or 24, entity.height or 24

    return Utils.rectangle(x, y, width, height), {Utils.rectangle(nodeX, nodeY, width, height)}
end

return TrapGate