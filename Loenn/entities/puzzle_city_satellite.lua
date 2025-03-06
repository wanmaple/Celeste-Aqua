local DrawableSprite = require("structs.drawable_sprite")

local PuzzleCitySatellite = {}

PuzzleCitySatellite.name = "Aqua/Puzzle City Satellite"
PuzzleCitySatellite.depth = 8999
PuzzleCitySatellite.nodeLineRenderType = "line"
PuzzleCitySatellite.nodeLimits = {1, 1}
PuzzleCitySatellite.fieldInformation = {
    puzzleId = {
        fieldType = "string",
    },
}
PuzzleCitySatellite.placements = {
    name = "Puzzle City Satellite",
    data = {
        puzzleId = "my_puzzle",
    },
}

local GEM_TEXTURE = "collectables/heartGem/0/00"
local DISH_TEXTURE = "objects/citysatellite/dish"
local LIGHT_TEXTURE = "objects/citysatellite/light"
local COMPUTER_TEXTURE = "objects/citysatellite/computer"
local COMPUTER_SCREEN_TEXTURE = "objects/citysatellite/computerscreen"

local COMPUTER_OFFSET_X, COMPUTER_OFFSET_Y = 32, 24

function PuzzleCitySatellite.sprite(room, entity)
    local dishSprite = DrawableSprite.fromTexture(DISH_TEXTURE, entity)
    dishSprite:setJustification(0.5, 1.0)

    local lightSprite = DrawableSprite.fromTexture(LIGHT_TEXTURE, entity)
    lightSprite:setJustification(0.5, 1.0)

    local computerSprite = DrawableSprite.fromTexture(COMPUTER_TEXTURE, entity)
    computerSprite:addPosition(COMPUTER_OFFSET_X, COMPUTER_OFFSET_Y)

    local computerScreenSprite = DrawableSprite.fromTexture(COMPUTER_SCREEN_TEXTURE, entity)
    computerScreenSprite:addPosition(COMPUTER_OFFSET_X, COMPUTER_OFFSET_Y)

    return {
        dishSprite, lightSprite, computerSprite, computerScreenSprite
    }
end

function PuzzleCitySatellite.nodeSprite(room, entity, node, nodeIndex)
    local gemSprite = DrawableSprite.fromTexture(GEM_TEXTURE, node)
    return gemSprite
end

function PuzzleCitySatellite.nodeRectangle(room, entity, node, nodeIndex)
    local gemSprite = DrawableSprite.fromTexture(GEM_TEXTURE, node)
    return gemSprite:getRectangle()
end

return PuzzleCitySatellite
