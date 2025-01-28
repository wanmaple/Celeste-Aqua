local DrawableNinePatch = require("structs.drawable_nine_patch")
local DrawableSprite = require("structs.drawable_sprite")
local Depths = require("consts.object_depths")

local OutlineDisplay = {}
OutlineDisplay.name = "Aqua/Outline Display"
OutlineDisplay.depth = Depths.SolidsBelow
OutlineDisplay.fieldInformation = {
    color = {
        fieldType = "color",
        useAlpha = false,
    },
}
OutlineDisplay.placements = {
    name = "Outline Display",
    data = {
        width = 24,
        height = 24,
        texture_path = "objects/acceleration_area/area",
        color = "ffffff",
    },
}

local blockNinePatchOptions = {
    mode = "border",
    borderMode = "repeat"
}

function OutlineDisplay.sprite(room, entity)
    local ninePatch = DrawableNinePatch.fromTexture(entity.texture_path, blockNinePatchOptions, entity.x, entity.y, entity.width, entity.height)
    local sprites = {}
    for _, sprite in ipairs(ninePatch:getDrawableSprite()) do
        sprite:setColor(entity.color)
        table.insert(sprites, sprite)
    end
    return sprites
end

return OutlineDisplay