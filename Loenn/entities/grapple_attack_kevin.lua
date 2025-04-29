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
    face_color = {
        fieldType = "color",
        useAlpha = false,
    },
}
GrappleAttackKevin.placements = {
    name = "Grapple Attack Kevin",
    data = {
        width = 24,
        height = 24,
        top = true,
        bottom = true,
        left = true,
        right = true,
        grapple_trigger = true,
        dash_trigger = true,
        no_return = false,
        edge_color = "0efefe",
        face_color = "ac5757",
    },
}

local TEXTURES = {
    BLOCK = "objects/grapple_kevin/block",
    TOP = "objects/grapple_kevin/block_top",
    BOTTOM = "objects/grapple_kevin/block_bottom",
    LEFT = "objects/grapple_kevin/block_left",
    RIGHT = "objects/grapple_kevin/block_right",
}

local CORNERS = {
    TL = "objects/grapple_kevin/corner_a",
    TR = "objects/grapple_kevin/corner_b",
    BL = "objects/grapple_kevin/corner_c",
    BR = "objects/grapple_kevin/corner_d",
}

local NINE_PATCH_OPTIONS = {
    mode = "border",
    borderMode = "repeat",
}

local KEVIN_COLOR = {98 / 255, 34 / 255, 43 / 255}
local SMALL_FACE_TEXTURE = "objects/grapple_kevin/face/idle_face"

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
    local faceTexture = SMALL_FACE_TEXTURE
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
    if entity.top and entity.left then
        local tl = DrawableSprite.fromTexture(CORNERS["TL"], { x = x + 16.0, y = y + 16.0, })
        tl:setColor(entity.edge_color)
        table.insert(sprites, tl)
    end
    if entity.top and entity.right then
        local tr = DrawableSprite.fromTexture(CORNERS["TR"], { x = x + width - 16.0, y = y + 16.0, })
        tr:setColor(entity.edge_color)
        table.insert(sprites, tr)
    end
    if entity.bottom and entity.left then
        local bl = DrawableSprite.fromTexture(CORNERS["BL"], { x = x + 16.0, y = y + height - 16.0, })
        bl:setColor(entity.edge_color)
        table.insert(sprites, bl)
    end
    if entity.bottom and entity.right then
        local br = DrawableSprite.fromTexture(CORNERS["BR"], { x = x + width - 16.0, y = y + height - 16.0, })
        br:setColor(entity.edge_color)
        table.insert(sprites, br)
    end
    local rectangle = DrawableRectangle.fromRectangle("fill", x + 2, y + 2, width - 4, height - 4, entity.face_color)
    local face = DrawableSprite.fromTexture(faceTexture, entity)
    face:setColor(entity.face_color)
    face:addPosition(math.floor(width / 2), math.floor(height / 2))
    table.insert(sprites, 1, rectangle:getDrawableSprite())
    table.insert(sprites, 2, face)
    return sprites
end

return GrappleAttackKevin