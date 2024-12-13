local Triggers = require("triggers")
local Depths = require("consts.object_depths")
local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableText = require("structs.drawable_text")

local BoosterFilter = {}

local PASS_TYPE = {
    "Green", "Red", "Both",
}
local ENTITY_COLORS = {
    "bc630e", "760ebc", "993865",
}
local PARTICLE_COLORS = {
    "bc630e", "760ebc", "993865",
}

BoosterFilter.name = "Aqua/Booster Filter"
BoosterFilter.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = false,
    },
    particle_color = {
        fieldType = "color",
        useAlpha = false,
    },
    can_pass = {
        options = PASS_TYPE,
        editable = false,
    },
}
BoosterFilter.placements = {}
for i, passType in ipairs(PASS_TYPE) do
    table.insert(BoosterFilter.placements, {
        name = string.format("Booster Filter (%s Pass)", passType),
        data = {
            width = 8,
            height = 8,
            color = ENTITY_COLORS[i],
            opacity = 0.15,
            particle_color = PARTICLE_COLORS[i],
            particle_opacity = 0.5,
            can_pass = passType,
        },
    })
end

function BoosterFilter.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local width = entity.width or 16
    local height = entity.height or 16

    local fillColor, borderColor = entity.color .. "26", "ffffff"
    local borderedRectangle = DrawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor)
    local textDrawable = DrawableText.fromText("Booster Filter", x, y, width, height, nil, Triggers.triggerFontSize)

    local drawables = borderedRectangle:getDrawableSprite()
    for _, drawable in ipairs(drawables) do
        drawable.depth = 0
    end
    table.insert(drawables, textDrawable)

    textDrawable.depth = 0
    return drawables
end

return BoosterFilter