local Triggers = require("triggers")
local Depths = require("consts.object_depths")
local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableText = require("structs.drawable_text")

local HoldableGrappleInteractionAssigner = {}

HoldableGrappleInteractionAssigner.name = "Aqua/Holdable Grapple Interaction Assigner"
HoldableGrappleInteractionAssigner.placements = {}

local TEXTS = { "Theo Like", "Jelly Like", }
local MASS_LIST = { 2.0, 0.5, }
local STAMINA_COST_LIST = { 20.0, 10.0, }
for i, text in ipairs(TEXTS) do
    table.insert(HoldableGrappleInteractionAssigner.placements, {
        name = string.format("Holdable Grapple Interaction Assigner (%s)", text),
        data = {
            width = 16,
            height = 16,
            mass = MASS_LIST[i],
            stamina_cost = STAMINA_COST_LIST[i],
            blacklist = "",
        },
    })
end

function HoldableGrappleInteractionAssigner.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local width = entity.width or 16
    local height = entity.height or 16

    local fillColor, borderColor = "1515d088", "ffffff"
    local borderedRectangle = DrawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor)
    local textDrawable = DrawableText.fromText("Holdable Grapple Interaction", x, y, width, height, nil, Triggers.triggerFontSize)

    local drawables = borderedRectangle:getDrawableSprite()
    for _, drawable in ipairs(drawables) do
        drawable.depth = Depths.triggers
    end
    table.insert(drawables, textDrawable)

    textDrawable.depth = Depths.triggers - 1

    return drawables
end

return HoldableGrappleInteractionAssigner