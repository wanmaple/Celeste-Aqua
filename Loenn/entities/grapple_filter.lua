local Triggers = require("triggers")
local Depths = require("consts.object_depths")
local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableText = require("structs.drawable_text")

local GrappleFilter = {}

GrappleFilter.name = "Aqua/Grapple Filter"
GrappleFilter.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = false,
    },
    particle_color = {
        fieldType = "color",
        useAlpha = false,
    },
    land_sound_index = {
        fieldType = "integer",
    },
}
GrappleFilter.placements = {
    name = "Grapple Filter",
    data = {
        width = 8,
        height = 8,
        color = "757575",
        opacity = 0.15,
        particle_color = "ffffff",
        particle_opacity = 0.5,
        collide_solids = false,
        land_sound_index = 11,
    },
}

function GrappleFilter.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local width = entity.width or 16
    local height = entity.height or 16

    local fillColor, borderColor = entity.color .. "26", "ffffff"
    local borderedRectangle = DrawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor)
    local textDrawable = DrawableText.fromText("Grapple Filter", x, y, width, height, nil, Triggers.triggerFontSize)

    local drawables = borderedRectangle:getDrawableSprite()
    for _, drawable in ipairs(drawables) do
        drawable.depth = 0
    end
    table.insert(drawables, textDrawable)

    textDrawable.depth = 0
    return drawables
end

return GrappleFilter