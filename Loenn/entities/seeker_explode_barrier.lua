local Triggers = require("triggers")
local Depths = require("consts.object_depths")
local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableText = require("structs.drawable_text")

local SeekerExplodeBarrier = {}

SeekerExplodeBarrier.name = "Aqua/Seeker Explode Barrier"
SeekerExplodeBarrier.placements = {
    name = "Seeker Explode Barrier",
    data = {
        width = 8,
        height = 8,
    },
}

function SeekerExplodeBarrier.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local width = entity.width or 16
    local height = entity.height or 16

    local fillColor, borderColor = "ff000026", "ffffff"
    local borderedRectangle = DrawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor)
    local textDrawable = DrawableText.fromText("Seeker Explode Barrier", x, y, width, height, nil, Triggers.triggerFontSize)

    local drawables = borderedRectangle:getDrawableSprite()
    for _, drawable in ipairs(drawables) do
        drawable.depth = 0
    end
    table.insert(drawables, textDrawable)

    textDrawable.depth = 0
    return drawables
end

return SeekerExplodeBarrier