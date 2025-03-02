local Triggers = require("triggers")
local DrawableSprite = require("structs.drawable_sprite")
local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableText = require("structs.drawable_text")
local Utils = require("utils")

local InvisibleGrappleAttractor = {}

local SHAPES = {
    "Circle",
    "Rectangle",
}

InvisibleGrappleAttractor.name = "Aqua/Invisible Grapple Attractor"
InvisibleGrappleAttractor.depth = 8995
InvisibleGrappleAttractor.justification = { 0.5, 0.5, }
InvisibleGrappleAttractor.fieldInformation = {
    shape = {
        options = SHAPES,
        editable = false,
    },
    radius_in_tiles = {
        fieldType = "integer",
    },
}
InvisibleGrappleAttractor.placements = {}
for _, shape in ipairs(SHAPES) do
    table.insert(InvisibleGrappleAttractor.placements, {
        name = string.format("Invisible Grapple Attractor (%s)", shape),
        data = {
            width = 32,
            height = 32,
            shape = shape,
            radius_in_tiles = 2,
            attach_to_solid = true,
        },
    })
end

function InvisibleGrappleAttractor.sprite(room, entity)
    local sprites = {}
    local centerTexture = "objects/hook_magnet/center_in_leonn"
    local centerSprite = DrawableSprite.fromTexture(centerTexture, entity)
    centerSprite:setJustification(0.5, 0.5)
    if entity.shape == "Circle" then
        local rangeTexture = "objects/hook_magnet/circle_in_leonn"
        local range = DrawableSprite.fromTexture(rangeTexture, entity)
        local radiusInTiles = entity.radius_in_tiles or 2
        range:setScale(radiusInTiles * 2, radiusInTiles * 2)
        range:setColor("d000d044")
        table.insert(sprites, range)
    elseif entity.shape == "Rectangle" then
        local x = entity.x or 0
        local y = entity.y or 0
        local width = entity.width or 32
        local height = entity.height or 32
        local fillColor, borderColor = "d000d044", "ffffff"
        local borderedRectangle = DrawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor)
        local textDrawable = DrawableText.fromText("Invisible Attractor", x, y, width, height, nil, Triggers.triggerFontSize)
        local drawables = borderedRectangle:getDrawableSprite()
        for _, drawable in ipairs(drawables) do
            table.insert(sprites, drawable)
        end
        centerSprite:setPosition(x + width * 0.5, y + height * 0.5)
    end
    table.insert(sprites, centerSprite)
    return sprites
end

function InvisibleGrappleAttractor.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    if entity.shape == "Circle" then
        local radius = math.max((entity.radius_in_tiles or 2), 1) * 8.0
        return Utils.rectangle(x - radius, y - radius, radius * 2.0, radius * 2.0)
    else
        local width = entity.width or 32
        local height = entity.height or 32
        return Utils.rectangle(x, y, width, height)
    end
end

return InvisibleGrappleAttractor