local Triggers = require("triggers")
local Depths = require("consts.object_depths")
local DrawableRectangle = require("structs.drawable_rectangle")
local DrawableText = require("structs.drawable_text")

local GrappleInteractionAssigner = {}

local ASSIGN_OPTIONS = {
    "GrabToPlayer",
    -- "PullPlayer",
}

GrappleInteractionAssigner.name = "Aqua/Grapple Interaction Assigner"
GrappleInteractionAssigner.fieldInformation = {
    interaction_type = {
        options = ASSIGN_OPTIONS,
        editable = false,
    },
}
GrappleInteractionAssigner.placements = {}
for _, option in ipairs(ASSIGN_OPTIONS) do
    table.insert(GrappleInteractionAssigner.placements, {
        name = string.format("Simple Grapple Interaction Assigner (%s)", option),
        data = {
            width = 16,
            height = 16,
            interaction_type = option,
            blacklist = "",
        },
    })
end

function GrappleInteractionAssigner.sprite(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0

    local width = entity.width or 16
    local height = entity.height or 16

    local fillColor, borderColor = "d0151588", "ffffff"
    local borderedRectangle = DrawableRectangle.fromRectangle("bordered", x, y, width, height, fillColor, borderColor)
    local textDrawable = DrawableText.fromText("Simple Grapple Interaction", x, y, width, height, nil, Triggers.triggerFontSize)

    local drawables = borderedRectangle:getDrawableSprite()
    for _, drawable in ipairs(drawables) do
        drawable.depth = Depths.triggers
    end
    table.insert(drawables, textDrawable)

    textDrawable.depth = Depths.triggers - 1

    return drawables
end

return GrappleInteractionAssigner