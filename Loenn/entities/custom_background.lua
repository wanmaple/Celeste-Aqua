local Triggers = require("triggers")
local Depths = require("consts.object_depths")
local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableText = require("structs.drawable_text")

local CustomBackground = {}

CustomBackground.name = "Aqua/Custom Background"
CustomBackground.placements = {
    name = "Custom Background",
    data = {
        width = 8,
        height = 8,
        fx = "",
    },
}

function CustomBackground.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local width = entity.width or 16
    local height = entity.height or 16

    local fillColor, borderColor = "00000000", "ffffff"
    local borderedRectangle = DrawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor)
    local textDrawable = DrawableText.fromText("Custom BG", x, y, width, height, nil, Triggers.triggerFontSize)

    local drawables = borderedRectangle:getDrawableSprite()
    for _, drawable in ipairs(drawables) do
        drawable.depth = Depths.triggers
    end
    table.insert(drawables, textDrawable)

    textDrawable.depth = Depths.triggers - 1

    return drawables
end

return CustomBackground