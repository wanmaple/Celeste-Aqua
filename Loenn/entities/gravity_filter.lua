local Triggers = require("triggers")
local Depths = require("consts.object_depths")
local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableText = require("structs.drawable_text")

local GravityFilter = {}

local GRAVITY_OPTIONS = {
    "Normal",
    "Inverted",
}

GravityFilter.name = "Aqua/Gravity Filter"
GravityFilter.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = false,
    },
    particle_color = {
        fieldType = "color",
        useAlpha = false,
    },
    gravity = {
        options = GRAVITY_OPTIONS,
        editable = false,
    },
}
GravityFilter.placements = {}
for i, gravityType in ipairs(GRAVITY_OPTIONS) do
    table.insert(GravityFilter.placements, {
        name = string.format("Gravity Filter (%s)", gravityType),
        data = {
            width = 8,
            height = 8,
            color = gravityType == "Inverted" and "3a628c" or "7d3d3d",
            active_opacity = 0.15,
            solidify_opacity = 0.8,
            particle_color = gravityType == "Inverted" and "007bff" or "ff0000",
            particle_opacity = 0.5,
            gravity = gravityType,
        },
    })
end

function GravityFilter.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local width = entity.width or 16
    local height = entity.height or 16

    local fillColor, borderColor = (entity.gravity == "Inverted" and "007bff" or "ff0000") .. "26", "ffffff"
    local borderedRectangle = DrawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor)
    local textDrawable = DrawableText.fromText("Gravity Filter", x, y, width, height, nil, Triggers.triggerFontSize)

    local drawables = borderedRectangle:getDrawableSprite()
    for _, drawable in ipairs(drawables) do
        drawable.depth = 0
    end
    table.insert(drawables, textDrawable)

    textDrawable.depth = 0
    return drawables
end

return GravityFilter