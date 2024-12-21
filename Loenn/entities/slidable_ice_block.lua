local DrawableNinePatch = require("structs.drawable_nine_patch")
local Depths = require("consts.object_depths")

local SlidableIceBlock = {}
SlidableIceBlock.name = "Aqua/Slidable Ice Block"
SlidableIceBlock.depth = 8995
SlidableIceBlock.placements = {
    name = "Slidable Ice Block",
    data = {
        width = 24,
        height = 24,
        hook_smooth = 2.5,
    },
}

local blockNinePatchOptions = {
    mode = "fill",
    borderMode = "repeat"
}

local TEXTURE = "objects/ice_block/ice_9tile"

function SlidableIceBlock.sprite(room, entity)
    local ninePatch = DrawableNinePatch.fromTexture(TEXTURE, blockNinePatchOptions, entity.x, entity.y, entity.width, entity.height)
    local sprites = {}
    for _, sprite in ipairs(ninePatch:getDrawableSprite()) do
        sprite:setColor(entity.border_color)
        table.insert(sprites, sprite)
    end
    table.insert(sprites, arrow)
    return sprites
end

return SlidableIceBlock