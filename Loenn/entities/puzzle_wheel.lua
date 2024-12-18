local DrawableSprite = require("structs.drawable_sprite")
local Utils = require("utils")

local PuzzleWheel = {}
PuzzleWheel.name = "Aqua/Puzzle Wheel"
PuzzleWheel.justification = {0.5, 0.5}
PuzzleWheel.depth = 8999
PuzzleWheel.nodeLimits = {1, -1}
PuzzleWheel.fieldInformation = {
    puzzleId = {
        fieldType = "string",
    },
}
PuzzleWheel.placements = {
    name = "Puzzle Wheel",
    data = {
        width = 32,
        height = 32,
        puzzleId = "my_puzzle",
        flag = "Rod1",
        time_limit = 30.0,
    }
}

local WHEEL_BG_TEXTURE = "objects/puzzle_wheel/background"
local WHEEL_PIN_TEXTURE = "objects/puzzle_wheel/pin"
local GEM_TEXTURE = "collectables/heartGem/0/00"

function PuzzleWheel.sprite(room, entity)
    local spriteBg = DrawableSprite.fromTexture(WHEEL_BG_TEXTURE, entity)
    spriteBg:setJustification(0.5, 0.5)
    spriteBg:setColor("5a5a5a")
    local spritePin = DrawableSprite.fromTexture(WHEEL_PIN_TEXTURE, entity)
    spritePin:setJustification(0.5, 0.5)
    spritePin:setColor("05ecfb")
    return { spriteBg, spritePin, }
end

function PuzzleWheel.rectangle(room, entity)
    local size = 40
    local mainRectangle = Utils.rectangle(entity.x - size * 0.5, entity.y - size * 0.5, size, size)
    return mainRectangle
end

function PuzzleWheel.nodeSprite(room, entity, node, nodeIndex)
    local gemSprite = DrawableSprite.fromTexture(GEM_TEXTURE, node)
    return gemSprite
end

function PuzzleWheel.nodeRectangle(room, entity, node, nodeIndex)
    local gemSprite = DrawableSprite.fromTexture(GEM_TEXTURE, node)
    return gemSprite:getRectangle()
end

return PuzzleWheel