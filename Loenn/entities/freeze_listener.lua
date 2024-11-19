local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableText = require("structs.drawable_text")
local Depths = require("consts.object_depths")

local FreezeListener = {}

FreezeListener.name = "Aqua/Freeze Listener"
FreezeListener.placements = {
    name = "Freeze Listener",
    data = {
        inputs = "UpRight,mod:Aqua/ThrowHook",
        width = 8,
        height = 8,
    },
}

function FreezeListener.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local width = entity.width or 16
    local height = entity.height or 16

    local fillColor, borderColor, textColor = "19f00688", "ffffff", "ffffff"
    local borderedRectangle = DrawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor)
    -- local textDrawable = DrawableText.fromText("Freeze Listener", x, y, width, height, nil, Triggers.triggerFontSize, textColor)

    local drawables = borderedRectangle:getDrawableSprite()
    for _, drawable in ipairs(drawables) do
        drawable.depth = Depths.triggers
    end
    -- table.insert(drawables, textDrawable)

    -- textDrawable.depth = Depths.triggers - 1

    return drawables
end

return FreezeListener