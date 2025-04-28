local DrawableNinePatch = require("structs.drawable_nine_patch")
local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableSprite = require("structs.drawable_sprite")

local GrappleAttackKevin = {}

GrappleAttackKevin.name = "Aqua/Grapple Attack Kevin"
GrappleAttackKevin.depth = 0
GrappleAttackKevin.warnBelowSize = { 24, 24 }
GrappleAttackKevin.fieldInformation = {
    edge_color = {
        fieldType = "color",
        useAlpha = false,
    },
    fill_color = {
        fieldType = "color",
        useAlpha = false,
    },
}
GrappleAttackKevin.placements = {
    name = "Grapple Attack Kevin",
    data = {
        width = 24,
        height = 24,
        chillout = false,
        top = true,
        bottom = true,
        left = true,
        right = true,
        grapple_trigger = true,
        dash_trigger = true,
        no_return = false,
        edge_color = "0efefe",
    },
}

local TEXTURES = {
    BLOCK = "objects/grapple_kevin/block",
    TOP = "objects/grapple_kevin/block_top",
    BOTTOM = "objects/grapple_kevin/block_bottom",
    LEFT = "objects/grapple_kevin/block_left",
    RIGHT = "objects/grapple_kevin/block_right",
}

local NINE_PATCH_OPTIONS = {
    mode = "border",
    borderMode = "repeat",
}

local KEVIN_COLOR = {98 / 255, 34 / 255, 43 / 255}
local SMALL_FACE_TEXTURE = "objects/crushblock/idle_face"
local GIANT_FACE_TEXTURE = "objects/crushblock/giant_block00"

local function addNinePatchToArray(ninePatch, array, color)
    local sprites = ninePatch:getDrawableSprite()
    for _, sprite in ipairs(sprites) do
        sprite:setColor(color or "ffffff")
        table.insert(array, sprite)
    end
end

function GrappleAttackKevin.sprite(room, entity)
    local sprites = {}
    local x, y = entity.x or 0, entity.y or 0
    local width, height = entity.width or 24, entity.height or 24
    local chillout = entity.chillout
    local giant = height >= 48 and width >= 48 and chillout
    local faceTexture = giant and GIANT_FACE_TEXTURE or SMALL_FACE_TEXTURE
    local idle = DrawableNinePatch.fromTexture(TEXTURES["BLOCK"], NINE_PATCH_OPTIONS, x, y, width, height)
    addNinePatchToArray(idle, sprites)
    if entity.top then
        local top = DrawableNinePatch.fromTexture(TEXTURES["TOP"], NINE_PATCH_OPTIONS, x, y, width, height)
        addNinePatchToArray(top, sprites, entity.edge_color)
    end
    if entity.bottom then
        local bottom = DrawableNinePatch.fromTexture(TEXTURES["BOTTOM"], NINE_PATCH_OPTIONS, x, y, width, height)
        addNinePatchToArray(bottom, sprites, entity.edge_color)
    end
    if entity.left then
        local left = DrawableNinePatch.fromTexture(TEXTURES["LEFT"], NINE_PATCH_OPTIONS, x, y, width, height)
        addNinePatchToArray(left, sprites, entity.edge_color)
    end
    if entity.right then
        local right = DrawableNinePatch.fromTexture(TEXTURES["RIGHT"], NINE_PATCH_OPTIONS, x, y, width, height)
        addNinePatchToArray(right, sprites, entity.edge_color)
    end
    local rectangle = DrawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, KEVIN_COLOR)
    local face = DrawableSprite.fromTexture(faceTexture, entity)
    face:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, 1, rectangle:getDrawableSprite())
    table.insert(sprites, 2, face)
    return sprites
end

return GrappleAttackKevin