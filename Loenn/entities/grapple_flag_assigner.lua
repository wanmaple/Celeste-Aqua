local Triggers = require("triggers")
local Depths = require("consts.object_depths")
local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableText = require("structs.drawable_text")

local GrappleFlagAssigner = {}

GrappleFlagAssigner.name = "Aqua/Grapple Flag Assigner"
GrappleFlagAssigner.placements = {
    name = "Grapple Flag Assigner",
    data = {
        width = 16,
        height = 16,
        flag = "ATTACH_FLAG1",
        state = true,
        attach = true,
        blacklist = "",
    },
}

function GrappleFlagAssigner.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local width = entity.width or 16
    local height = entity.height or 16

    local fillColor, borderColor = "15d01588", "ffffff"
    local borderedRectangle = DrawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor)
    local textDrawable = DrawableText.fromText("Grapple Flag", x, y, width, height, nil, Triggers.triggerFontSize)

    local drawables = borderedRectangle:getDrawableSprite()
    for _, drawable in ipairs(drawables) do
        drawable.depth = Depths.triggers
    end
    table.insert(drawables, textDrawable)

    textDrawable.depth = Depths.triggers - 1

    return drawables
end

return GrappleFlagAssigner