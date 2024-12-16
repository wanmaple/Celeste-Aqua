
local DrawableNinePatch = require("structs.drawable_nine_patch")
local DrawableSprite = require("structs.drawable_sprite")
local Depths = require("consts.object_depths")

local DIRECTIONS = {
    "Up", "Down", "Left", "Right",
}

local AccelerationArea = {}
AccelerationArea.name = "Aqua/Acceleration Area"
AccelerationArea.depth = Depths.SolidsBelow
AccelerationArea.fieldInformation = {
    direction = {
        options = DIRECTIONS,
        editable = false,
    },
    border_color = {
        fieldType = "color",
        useAlpha = false,
    },
    arrow_color = {
        fieldType = "color",
        useAlpha = false,
    },
    blink_border_color = {
        fieldType = "color",
        useAlpha = false,
    },
    blink_arrow_color = {
        fieldType = "color",
        useAlpha = false,
    },
}
AccelerationArea.placements = {}
for _, dir in ipairs(DIRECTIONS) do
    table.insert(AccelerationArea.placements, {
        name = string.format("Acceleration Area (%s)", dir),
        data = {
            width = 24,
            height = 24,
            direction = dir,
            border_color = "4e5cff",
            arrow_color = "4e5cff",
            blink_border_color = "ffffff",
            blink_arrow_color = "ffffff",
            blink_duration = 0.7,
        },
    })
end

local blockNinePatchOptions = {
    mode = "border",
    borderMode = "repeat"
}

local TEXTURE_BORDER = "objects/acceleration_area/area"
local TEXTURE_ARROW = "objects/acceleration_area/icon00"

function AccelerationArea.sprite(room, entity)
    local ninePatch = DrawableNinePatch.fromTexture(TEXTURE_BORDER, blockNinePatchOptions, entity.x, entity.y, entity.width, entity.height)
    local arrow = DrawableSprite.fromTexture(TEXTURE_ARROW, entity)
    arrow:addPosition(entity.width * 0.5, entity.height * 0.5)
    arrow:setColor(entity.arrow_color)
    if entity.direction == "Up" then
        arrow.rotation = -math.pi * 0.5
    elseif entity.direction == "Down" then
        arrow.rotation = math.pi * 0.5
    elseif entity.direction == "Left" then
        arrow.rotation = math.pi
    end
    local sprites = {}
    for _, sprite in ipairs(ninePatch:getDrawableSprite()) do
        sprite:setColor(entity.border_color)
        table.insert(sprites, sprite)
    end
    table.insert(sprites, arrow)
    return sprites
end

return AccelerationArea