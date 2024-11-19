local DrawableSprite = require("structs.drawable_sprite")

local PuzzleLight = {}

PuzzleLight.name = "Aqua/Puzzle Light"
PuzzleLight.texture = "objects/common/puzzle_light"
PuzzleLight.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = false,
    },
    sequence = {
        fieldType = "integer",
    },
    puzzleId = {
        fieldType = "string",
    },
}
PuzzleLight.placements = {
    name = "Puzzle Light",
    data = {
        puzzleId = "my_puzzle",
        sequence = 0,
        on = false,
        color = "ffffff",
    },
}

local PREDESTAL_TEXTURE = "objects/puzzle_light/pedestal"
local BULB_TEXTURE = "objects/puzzle_light/bulb00"
local LIT_TEXTURE = "objects/puzzle_light/lit"

function PuzzleLight.sprite(room, entity)
    local pedestal = DrawableSprite.fromTexture(PREDESTAL_TEXTURE, entity)
    pedestal:setJustification(0.5, 0.5)
    pedestal:setColor("ffffff")

    local bulb = DrawableSprite.fromTexture(BULB_TEXTURE, entity)
    bulb:setJustification(0.5, 0.5)
    bulb:setColor(entity.color)

    local sprites = { pedestal, bulb }
    if entity.on then
        local lit = DrawableSprite.fromTexture(LIT_TEXTURE, entity)
        lit:setJustification(0.5, 0.5)
        lit:setColor(entity.color)
        table.insert(sprites, lit)
    end

    return sprites
end

return PuzzleLight